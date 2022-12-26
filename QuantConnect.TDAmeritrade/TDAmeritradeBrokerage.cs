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
    /// <summary>
    /// TD Ameritrade Brokerage implementation
    /// </summary>
    [BrokerageFactory(typeof(TDAmeritradeBrokerageFactory))]
    public partial class TDAmeritradeBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private string _consumerKey;
        private string _accessToken;
        private string _accountNumber;
        /// <summary>
        /// The Refresh Token is lived for 90 days.
        /// </summary>
        private string _refreshToken;

        private string _restApiUrl = "https://api.tdameritrade.com/v1/";
        /// <summary>
        /// WebSocekt URL
        /// We can get url from GetUserPrincipals() mthd
        /// </summary>
        private string _wsUrl = "wss://streamer-ws.tdameritrade.com/ws";

        private IDataAggregator _aggregator;
        private readonly IAlgorithm _algorithm;
        private readonly IOrderProvider _orderProvider;

        private TDAmeritradeSymbolMapper _symbolMapper;

        /// <summary>
        /// Thread synchronization event, for successful Place Order
        /// </summary>
        private ManualResetEvent _onSumbitOrderWebSocketResponseEvent = new ManualResetEvent(false);

        /// <summary>
        /// Thread synchronization event, for successful Update Order
        /// </summary>
        private ManualResetEvent _onUpdateOrderWebSocketResponseEvent = new ManualResetEvent(false);

        /// <summary>
        /// Thread synchronization event, for successful Place Order in Lean 
        /// </summary>
        private ManualResetEvent _onPlaceOrderBrokerageIdResponseEvent = new ManualResetEvent(true);

        /// <summary>
        /// Creates a new TDAmeritradeBrokerage
        /// </summary>
        public TDAmeritradeBrokerage() : base("TD Ameritrade")
        {
        }

        /// <summary>
        /// Creates a new TDAmeritradeBrokerage
        /// </summary>
        public TDAmeritradeBrokerage(
            string consumerKey,
            string accessToken,
            string accountNumber,
            IAlgorithm algorithm,
            IDataAggregator aggregator,
            IOrderProvider orderProvider,
            IMapFileProvider mapFileProvider) : base("TD Ameritrade")
        {
            _algorithm = algorithm;
            _orderProvider = orderProvider;

            Initialize(consumerKey, accessToken, accountNumber, mapFileProvider, aggregator);
        }

        #region TD Ameritrade client

        private T? Execute<T>(RestRequest request)
        {
            var untypedResponse = RestClient.Execute(request);

            if (!untypedResponse.IsSuccessful)
            {
                if (untypedResponse.Content.Contains("The access token being passed has expired or is invalid")) // The Access Token has invalid
                {
                    // Remove default parameter, to we get new accessToken correctly
                    RestClient.RemoveDefaultParameter("Authorization");
                    // Get new access token
                    // REMARK: Authorization token is lived for 1800 seconds (30 minutes)
                    var accessTokens = PostAccessToken(GrantType.RefreshToken, string.Empty);
                    // Update access token in request parameter
                    RestClient.AddOrUpdateDefaultParameter(new Parameter("Authorization", accessTokens.TokenType + " " + accessTokens.AccessToken, ParameterType.HttpHeader));
                    untypedResponse = RestClient.Execute(request);
                }
                else if (request.Resource == "oauth2/token")
                {
                    throw new BrokerageException($"TDAmeritradeBrokerage.Execute.{request.Resource}: authorization request is invalid, Response:{untypedResponse.Content}");
                }
                else if (!string.IsNullOrEmpty(untypedResponse.Content))
                {
                    var fault = JsonConvert.DeserializeObject<ErrorModel>(untypedResponse.Content);
                    Log.Error($"{"TDAmeritrade.Execute." + request.Resource}(2): Parameters: {string.Join(",", request.Parameters.Select(x => x.Name + ": " + x.Value))} Response: {untypedResponse.Content}");
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "TDAmeritradeFault", "Error Detail from object"));
                }
            }

            try
            {
                // api sometimes returns message in response 
                if (typeof(T) == typeof(String))
                {
                    return (T)(object)untypedResponse.Content;
                }

                return JsonConvert.DeserializeObject<T>(untypedResponse.Content);
            }
            catch (Exception e)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "JsonError", $"Error deserializing message: {untypedResponse.Content} Error: {e.Message}"));
                return default;
            }
        }

        public override bool PlaceOrder(Order order)
        {
            _onPlaceOrderBrokerageIdResponseEvent.Reset();
            var placeOrderResponse = PostPlaceOrder(order);

            if (!string.IsNullOrEmpty(placeOrderResponse))
            {
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, OrderFee.Zero, "TDAmeritrade Order Event") { Status = OrderStatus.Invalid, Message = placeOrderResponse });
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, placeOrderResponse));
                _onPlaceOrderBrokerageIdResponseEvent.Set();
                return false;
            }

            // If we haven't gotten response from WebSocket than we stop our algorithm.
            if(!WaitWebSocketResponse(_onSumbitOrderWebSocketResponseEvent, OrderStatus.Submitted))
            {
                _onPlaceOrderBrokerageIdResponseEvent.Set();
                return false;
            }

            // After we have gotten websocket, we will dequeue order from queue
            _submittedOrderIds.TryDequeue(out var orderIdResponse);
            order.BrokerId.Add(orderIdResponse);
        
            OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, OrderFee.Zero, "TDAmeritrade Order Event SubmitNewOrder") 
            { Status = OrderStatus.Submitted });
            Log.Trace($"Order submitted successfully - OrderId: {order.Id}");
            
            _onPlaceOrderBrokerageIdResponseEvent.Set();
            return true;
        }

        /// <summary>
        /// Updates the order with the same id
        /// </summary>
        /// <param name="order">The new order information</param>
        /// <returns>True if the request was made for the order to be updated, false otherwise</returns>
        public override bool UpdateOrder(Order order)
        {
            Log.Trace("TDAmeritradeBrokerage.UpdateOrder(): " + order);

            if (!order.BrokerId.Any())
            {
                // we need the brokerage order id in order to perform an update
                Log.Trace("TDAmeritradeBrokerage.UpdateOrder(): Unable to update order without BrokerId.");
                return false;
            }

            var replaceOrderResponse = ReplaceOrder(order);

            if (!string.IsNullOrEmpty(replaceOrderResponse))
            {
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, OrderFee.Zero, "TDAmeritrade Order Event") { Status = OrderStatus.Invalid, Message = replaceOrderResponse });
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, replaceOrderResponse));
                return false;
            }

            // If we haven't gotten response from WebSocket than we stop our algorithm.
            return WaitWebSocketResponse(_onUpdateOrderWebSocketResponseEvent, OrderStatus.UpdateSubmitted);
        }

        private bool WaitWebSocketResponse(ManualResetEvent webSocketEvent, OrderStatus orderStatus)
        {
            if (!webSocketEvent.WaitOne(TimeSpan.FromSeconds(10)))
            {
                var error = $"TDAmeritradeBrokerage didn't get response from websocket. Order Status: {orderStatus}";
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1, error));
                return false;
            }
            webSocketEvent.Reset();
            return true;
        }

        public override bool CancelOrder(Order order)
        {
            var success = new List<bool>();

            foreach (var id in order.BrokerId)
            {
                var isCancelSuccess = CancelOrder(id);

                success.Add(isCancelSuccess);
            }

            return success.All(a => a);
        }

        public override List<Order> GetOpenOrders()
        {
            var orders = new List<Order>();

            var openOrders = GetOrdersByPath(toEnteredTime: DateTime.Today, orderStatusType: OrderStatusType.Working);

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

            var quotes = GetQuotesLastPrice(positions.Select(x => x.ProjectedBalances.Symbol));

            foreach (var hold in positions)
            {
                var brokerageSymbol = hold.ProjectedBalances.Symbol;
                var leanSecurityType = hold.ProjectedBalances.AssetType.ConvertBrokerageSecurityTypeToLeanSecurityType();

                var symbol = _symbolMapper.GetLeanSymbol(brokerageSymbol, leanSecurityType, Market.USA);

                holdings.Add(new Holding()
                {
                    Symbol = symbol,
                    AveragePrice = hold.AveragePrice,
                    MarketPrice = quotes[brokerageSymbol],
                    Quantity = hold.LongQuantity + hold.ShortQuantity,
                    MarketValue = hold.MarketValue
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

        private void Initialize(string consumerKey, string accessToken, string accountNumber, IMapFileProvider mapFileProvider, IDataAggregator aggregator)
        {
            if (IsInitialized)
            {
                return;
            }

            _aggregator = aggregator;
            _consumerKey = consumerKey;
            _accessToken = accessToken;
            _accountNumber = accountNumber;
            _symbolMapper = new TDAmeritradeSymbolMapper(mapFileProvider);

            RestClient = new RestClient(_restApiUrl);

            if (!string.IsNullOrEmpty(_accessToken) && string.IsNullOrEmpty(_refreshToken))
            {
                _refreshToken = PostAccessToken(GrantType.AuthorizationCode, _accessToken).RefreshToken;
            }

            if (!string.IsNullOrEmpty(_refreshToken))
            {
                var accessTokens = PostAccessToken(GrantType.RefreshToken, string.Empty);
                RestClient.AddOrUpdateDefaultParameter(new Parameter("Authorization", accessTokens.TokenType + " " + accessTokens.AccessToken, ParameterType.HttpHeader));
            }

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
                var productId = 226;
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