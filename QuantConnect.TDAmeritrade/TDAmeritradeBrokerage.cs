/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Api;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Brokerages.TDAmeritrade.Utils;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using QuantConnect.Util;
using RestSharp;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    [BrokerageFactory(typeof(TDAmeritradeBrokerage))]
    public partial class TDAmeritradeBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private readonly string _consumerKey;
        private readonly string _codeFromUrl;
        private readonly string _accountNumber;
        private string _refreshToken;

        private string _restApiUrl = "https://api.tdameritrade.com/v1/";
        /// <summary>
        /// WebSocekt URL
        /// We can get url from GetUserPrincipals() mthd
        /// </summary>
        private string _wsUrl = "wss://streamer-ws.tdameritrade.com/ws";

        private readonly IAlgorithm _algorithm;
        private ISecurityProvider _securityProvider;
        private readonly IDataAggregator _aggregator;
        private readonly IOrderProvider _orderProvider;

        private readonly object _lockAccessCredentials = new object();
        private readonly FixedSizeHashQueue<int> _cancelledQcOrderIDs = new FixedSizeHashQueue<int>(10000);
        private readonly TDAmeritradeSymbolMapper _symbolMapper;

        public TDAmeritradeBrokerage() : base("TD Ameritrade")
        { }

        public TDAmeritradeBrokerage(
            string consumerKey,
            string codeFromUrl,
            string accountNumber,
            IAlgorithm algorithm,
            ISecurityProvider securityProvider,
            IDataAggregator aggregator,
            IOrderProvider orderProvider,
            IMapFileProvider mapFileProvider) : base("TD Ameritrade")
        {
            _consumerKey = consumerKey;
            _codeFromUrl = codeFromUrl;
            _accountNumber = accountNumber;
            _algorithm = algorithm;
            _securityProvider = securityProvider;
            _aggregator = aggregator;
            _orderProvider = orderProvider;
            _symbolMapper = new TDAmeritradeSymbolMapper(mapFileProvider);

            Initialize();
        }

        #region TD Ameritrade client

        private T Execute<T>(RestRequest request, string rootName = "")
        {
            var response = default(T);

            var method = "TDAmeritrade.Execute." + request.Resource;
            var parameters = request.Parameters.Select(x => x.Name + ": " + x.Value);

            lock (_lockAccessCredentials)
            {
                var raw = RestClient.Execute(request);

                if (!raw.IsSuccessful)
                {
                    if (raw.Content.Contains("The access token being passed has expired or is invalid")) // The Access Token has invalid
                    {
                        PostAccessToken(GrantType.RefreshToken, string.Empty);
                        Execute<T>(request, rootName);
                    }
                    else if (!string.IsNullOrEmpty(raw.Content))
                    {
                        var fault = JsonConvert.DeserializeObject<ErrorModel>(raw.Content);
                        Log.Error($"{method}(2): Parameters: {string.Join(",", parameters)} Response: {raw.Content}");
                        OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "TDAmeritradeFault", "Error Detail from object"));
                        return (T)(object)fault.Error;
                    }
                }

                try
                {
                    if (typeof(T) == typeof(String))
                        return (T)(object)raw.Content;

                    if (!string.IsNullOrEmpty(rootName))
                    {
                        if (TryDeserializeRemoveRoot(raw.Content, rootName, out response))
                        {
                            return response;
                        }
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<T>(raw.Content);
                    }
                }
                catch (Exception e)
                {
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "JsonError", $"Error deserializing message: {raw.Content} Error: {e.Message}"));
                }
            }

            return response;
        }

        public override bool PlaceOrder(Order order)
        {
            var orderLegCollection = new List<PlaceOrderLegCollectionModel>()
            {
                new PlaceOrderLegCollectionModel(
                    order.Direction.ConvertQCOrderDirectionToExchange(),
                    Math.Abs(order.Quantity),
                    new InstrumentPlaceOrderModel(order.Symbol.Value, order.Symbol.SecurityType.ToString().ToUpper())
                    )
            };

            var isOrderMarket = order.Type == Orders.OrderType.Market ? true : false;

            decimal limitPrice = 0m;
            if (!isOrderMarket)
            {
                limitPrice =
                    (order as LimitOrder)?.LimitPrice ??
                    (order as StopLimitOrder)?.LimitPrice ?? 0;
            }

            decimal stopPrice = 0m;
            if (order.Type == Orders.OrderType.StopLimit)
                stopPrice = (order as StopLimitOrder)?.StopPrice ?? 0;

            var response = PostPlaceOrder(order.Type.ConvertQCOrderTypeToExchange(),
                SessionType.Normal,
                DurationType.Day,
                OrderStrategyType.Single,
                orderLegCollection,
                isOrderMarket ? null : ComplexOrderStrategyType.None,
                limitPrice.RoundToSignificantDigits(4),
                stopPrice.RoundToSignificantDigits(4));

            if (!string.IsNullOrEmpty(response))
            {
                var orderFee = OrderFee.Zero;   
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, orderFee, "TDAmeritrade Order Event") { Status = OrderStatus.Invalid, Message = response });
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, response));
                return false;
            }

            return true;
        }

        public override bool UpdateOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public override bool CancelOrder(Order order)
        {
            var success = new List<bool>();

            foreach (var id in order.BrokerId)
            {
                var res = CancelOrder(id);

                if (res)
                {
                    success.Add(res);
                    OnOrderEvent(new OrderEvent(order,
                        DateTime.UtcNow,
                        OrderFee.Zero,
                        "TDAmeritrade Order Event")
                    { Status = OrderStatus.Canceled });
                }
            }

            return success.All(a => a);
        }

        public override List<Order> GetOpenOrders()
        {
            var orders = new List<Order>();

            var openOrders = GetOrdersByPath(toEnteredTime: DateTime.Today, orderStatusType: OrderStatusType.PendingActivation);

            foreach (var openOrder in openOrders)
            {
                orders.Add(openOrder.ConvertOrder());
            }

            return orders;
        }

        public override List<Holding> GetAccountHoldings()
        {
            var positions = GetAccount(_accountNumber).SecuritiesAccount.Positions;

            var holdings = new List<Holding>(positions.Count);

            foreach (var hold in positions)
            {
                var symbol = Symbol.Create(hold.ProjectedBalances.Symbol, SecurityType.Equity, Market.USA);

                holdings.Add(new Holding()
                {
                    Symbol = symbol,
                    AveragePrice = hold.AveragePrice,
                    MarketPrice = hold.MarketValue,
                    Quantity = hold.SettledLongQuantity + hold.SettledShortQuantity,
                    MarketValue = hold.MarketValue,
                    UnrealizedPnL = hold.CurrentDayProfitLossPercentage // % or $ - ?
                });
            }
            return holdings;
        }

        public override List<CashAmount> GetCashBalance()
        {
            var balance = GetAccount(_accountNumber).SecuritiesAccount.CurrentBalances.AvailableFunds;
            return new List<CashAmount>() { new CashAmount(balance, Currencies.USD) };
        }

        #endregion

        private void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            RestClient = new RestClient(_restApiUrl);

            if(string.IsNullOrEmpty(_refreshToken))
                _refreshToken = PostAccessToken(GrantType.AuthorizationCode, _codeFromUrl).RefreshToken;

            PostAccessToken(GrantType.RefreshToken, string.Empty);

            Initialize(_wsUrl, new WebSocketClientWrapper(), RestClient, null, null);

            WebSocket.Open += (sender, args) => { Login(); };
            
            var subscriptionManager = new EventBasedDataQueueHandlerSubscriptionManager();

            subscriptionManager.SubscribeImpl += (symbols, _) => Subscribe(symbols);
            subscriptionManager.UnsubscribeImpl += (symbols, _) => Unsubscribe(symbols);
            SubscriptionManager = subscriptionManager;


            ValidateSubscription();
        }

        private class ModulesReadLicenseRead : Api.RestResponse
        {
            [JsonProperty(PropertyName = "license")]
            public string License;
            [JsonProperty(PropertyName = "organizationId")]
            public string OrganizationId;
        }

        /// <summary>
        /// Validate the user of this project has permission to be using it via our web API.
        /// </summary>
        private static void ValidateSubscription()
        {
            try
            {
                var productId = 221; // TODO: it must change
                var userId = Config.GetInt("job-user-id");
                var token = Config.Get("api-access-token");
                var organizationId = Config.Get("job-organization-id", null);
                // Verify we can authenticate with this user and token
                var api = new ApiConnection(userId, token);
                if (!api.Connected)
                {
                    throw new ArgumentException("Invalid api user id or token, cannot authenticate subscription.");
                }
                // Compile the information we want to send when validating
                var information = new Dictionary<string, object>()
                {
                    {"productId", productId},
                    {"machineName", Environment.MachineName},
                    {"userName", Environment.UserName},
                    {"domainName", Environment.UserDomainName},
                    {"os", Environment.OSVersion}
                };
                // IP and Mac Address Information
                try
                {
                    var interfaceDictionary = new List<Dictionary<string, object>>();
                    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up))
                    {
                        var interfaceInformation = new Dictionary<string, object>();
                        // Get UnicastAddresses
                        var addresses = nic.GetIPProperties().UnicastAddresses
                            .Select(uniAddress => uniAddress.Address)
                            .Where(address => !IPAddress.IsLoopback(address)).Select(x => x.ToString());
                        // If this interface has non-loopback addresses, we will include it
                        if (!addresses.IsNullOrEmpty())
                        {
                            interfaceInformation.Add("unicastAddresses", addresses);
                            // Get MAC address
                            interfaceInformation.Add("MAC", nic.GetPhysicalAddress().ToString());
                            // Add Interface name
                            interfaceInformation.Add("name", nic.Name);
                            // Add these to our dictionary
                            interfaceDictionary.Add(interfaceInformation);
                        }
                    }
                    information.Add("networkInterfaces", interfaceDictionary);
                }
                catch (Exception)
                {
                    // NOP, not necessary to crash if fails to extract and add this information
                }
                // Include our OrganizationId is specified
                if (!string.IsNullOrEmpty(organizationId))
                {
                    information.Add("organizationId", organizationId);
                }
                var request = new RestRequest("modules/license/read", Method.POST) { RequestFormat = DataFormat.Json };
                request.AddParameter("application/json", JsonConvert.SerializeObject(information), ParameterType.RequestBody);
                api.TryRequest(request, out ModulesReadLicenseRead result);
                if (!result.Success)
                {
                    throw new InvalidOperationException($"Request for subscriptions from web failed, Response Errors : {string.Join(',', result.Errors)}");
                }

                var encryptedData = result.License;
                // Decrypt the data we received
                DateTime? expirationDate = null;
                long? stamp = null;
                bool? isValid = null;
                if (encryptedData != null)
                {
                    // Fetch the org id from the response if we are null, we need it to generate our validation key
                    if (string.IsNullOrEmpty(organizationId))
                    {
                        organizationId = result.OrganizationId;
                    }
                    // Create our combination key
                    var password = $"{token}-{organizationId}";
                    var key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
                    // Split the data
                    var info = encryptedData.Split("::");
                    var buffer = Convert.FromBase64String(info[0]);
                    var iv = Convert.FromBase64String(info[1]);
                    // Decrypt our information
                    using var aes = new AesManaged();
                    var decryptor = aes.CreateDecryptor(key, iv);
                    using var memoryStream = new MemoryStream(buffer);
                    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    using var streamReader = new StreamReader(cryptoStream);
                    var decryptedData = streamReader.ReadToEnd();
                    if (!decryptedData.IsNullOrEmpty())
                    {
                        var jsonInfo = JsonConvert.DeserializeObject<JObject>(decryptedData);
                        expirationDate = jsonInfo["expiration"]?.Value<DateTime>();
                        isValid = jsonInfo["isValid"]?.Value<bool>();
                        stamp = jsonInfo["stamped"]?.Value<int>();
                    }
                }
                // Validate our conditions
                if (!expirationDate.HasValue || !isValid.HasValue || !stamp.HasValue)
                {
                    throw new InvalidOperationException("Failed to validate subscription.");
                }

                var nowUtc = DateTime.UtcNow;
                var timeSpan = nowUtc - Time.UnixTimeStampToDateTime(stamp.Value);
                if (timeSpan > TimeSpan.FromHours(12))
                {
                    throw new InvalidOperationException("Invalid API response.");
                }
                if (!isValid.Value)
                {
                    throw new ArgumentException($"Your subscription is not valid, please check your product subscriptions on our website.");
                }
                if (expirationDate < nowUtc)
                {
                    throw new ArgumentException($"Your subscription expired {expirationDate}, please renew in order to use this product.");
                }
            }
            catch (Exception e)
            {
                Log.Error($"ValidateSubscription(): Failed during validation, shutting down. Error : {e.Message}");
                Environment.Exit(1);
            }
        }
    }
}
