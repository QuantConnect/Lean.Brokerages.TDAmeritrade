using Newtonsoft.Json;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Brokerages.TDAmeritrade.Utils;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using QuantConnect.Util;
using RestSharp;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    [BrokerageFactory(typeof(TDAmeritradeBrokerage))]
    public partial class TDAmeritradeBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private readonly string _consumerKey;
        private readonly string _refreshToken;
        private readonly string _callbackUrl;
        private readonly string _codeFromUrl;
        private readonly string _accountNumber;

        private string _restApiUrl = "https://api.tdameritrade.com/v1/";
        /// <summary>
        /// WebSocekt URL
        /// We can get url from GetUserPrincipals() mthd
        /// </summary>
        private string _wsUrl = "wss://streamer-ws.tdameritrade.com/ws";

        private readonly IAlgorithm _algorithm;
        private ISecurityProvider _securityProvider;
        private readonly IDataAggregator _aggregator;

        private readonly object _lockAccessCredentials = new object();
        private readonly FixedSizeHashQueue<int> _cancelledQcOrderIDs = new FixedSizeHashQueue<int>(10000);
        private readonly TDAmeritradeSymbolMapper _symbolMapper = new();

        public TDAmeritradeBrokerage() : base("TD Ameritrade")
        { }

        public TDAmeritradeBrokerage(
            string consumerKey,
            string refreshToken,
            string callbackUrl,
            string codeFromUrl,
            string accountNumber,
            IAlgorithm algorithm,
            ISecurityProvider securityProvider,
            IDataAggregator aggregator) : base("TD Ameritrade")
        {
            _consumerKey = consumerKey;
            _refreshToken = refreshToken;
            _callbackUrl = callbackUrl;
            _codeFromUrl = codeFromUrl;
            _accountNumber = accountNumber;
            _algorithm = algorithm;
            _securityProvider = securityProvider;
            _aggregator = aggregator;

            Initialize();
            //ValidateSubscription(); // Quant Connect api permission
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

            var orderFee = OrderFee.Zero;
            if (!string.IsNullOrEmpty(response))
            {
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, orderFee, "TDAmeritrade Order Event") { Status = OrderStatus.Invalid, Message = response });
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, response));
                return false;
            }

            var orderResponse = GetOrdersByPath().First();
            if (orderResponse.Status == OrderStatusType.Rejected)
            {
                var errorMessage = $"Reject reason: {orderResponse.StatusDescription}"; 
                OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, orderFee, "TDAmeritrade Order Event") { Status = OrderStatus.Invalid, Message = errorMessage });
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, errorMessage));
                return true;
            }

            order.BrokerId.Add(orderResponse.OrderId.ToStringInvariant());

            OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, orderFee, "TDAmeritrade Order Event") { Status = OrderStatus.Submitted });
            Log.Trace($"Order submitted successfully - OrderId: {order.Id}");

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

            PostAccessToken(GrantType.RefreshToken, string.Empty);

            Initialize(_wsUrl, new WebSocketClientWrapper(), RestClient, null, null);

            WebSocket.Open += (sender, args) => { Login(); };
            
            var subscriptionManager = new EventBasedDataQueueHandlerSubscriptionManager();

            subscriptionManager.SubscribeImpl += (symbols, _) => Subscribe(symbols);
            subscriptionManager.UnsubscribeImpl += (symbols, _) => Unsubscribe(symbols);
            SubscriptionManager = subscriptionManager;


            //ValidateSubscription(); // TODO: implement mthd
        }
    }
}
