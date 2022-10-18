using Newtonsoft.Json;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Brokerages.TDAmeritrade.Utils;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Util;
using RestSharp;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    [BrokerageFactory(typeof(TDAmeritradeBrokerage))]
    public partial class TDAmeritradeBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private string _accessToken = string.Empty;
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
        private readonly IDataAggregator _aggregator;

        private readonly object _lockAccessCredentials = new object();
        private readonly FixedSizeHashQueue<int> _cancelledQcOrderIDs = new FixedSizeHashQueue<int>(10000);

        public TDAmeritradeBrokerage() : base("TD Ameritrade")
        { }

        public TDAmeritradeBrokerage(
            string consumerKey,
            string refreshToken,
            string callbackUrl,
            string codeFromUrl,
            string accountNumber,
            IAlgorithm algorithm) : base("TD Ameritrade")
        {
            _consumerKey = consumerKey;
            _refreshToken = refreshToken;
            _callbackUrl = callbackUrl;
            _codeFromUrl = codeFromUrl;
            _accountNumber = accountNumber;
            _algorithm = algorithm;

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
                        _accessToken = string.Empty;
                        RestClient.AddOrUpdateDefaultParameter(new Parameter("Authorization", _accessToken, ParameterType.HttpHeader));
                        Execute<T>(request, rootName);
                    }
                    else if (!string.IsNullOrEmpty(raw.Content))
                    {
                        var fault = JsonConvert.DeserializeObject<ErrorModel>(raw.Content);
                        Log.Error($"{method}(2): Parameters: {string.Join(",", parameters)} Response: {raw.Content}");
                        OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "TDAmeritradeFault", "Error Detail from object"));
                        return response;
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
            throw new NotImplementedException();
        }

        public override bool UpdateOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public override bool CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public override List<Order> GetOpenOrders()
        {
            var orders = new List<Order>();

            var openOrders = GetOrdersByPath(toEnteredTime: DateTime.Today, orderStatusType: OrderStatusType.PendingActivation);

            foreach (var openOrder in openOrders)
            {
                // make sure our internal collection is up to date as well
                //UpdateCachedOpenOrder(openOrder.Id, openOrder);
                orders.Add(ConvertOrder(openOrder));
            }

            return orders;
        }

        public override List<Holding> GetAccountHoldings()
        {
            throw new NotImplementedException();
        }

        public override List<CashAmount> GetCashBalance()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            RestClient = new RestClient(_restApiUrl);

            if (string.IsNullOrEmpty(_accessToken))
            {
                PostAccessToken(GrantType.RefreshToken, string.Empty);
                RestClient.AddDefaultHeader("Authorization", _accessToken);
            }

            Initialize(_wsUrl, new WebSocketClientWrapper(), RestClient, null, null);

            var subscriptionManager = new EventBasedDataQueueHandlerSubscriptionManager();
            subscriptionManager.SubscribeImpl += (symbols, _) => Subscribe(symbols);
            subscriptionManager.UnsubscribeImpl += (symbols, _) => Unsubscribe(symbols);
            SubscriptionManager = subscriptionManager;

            //ValidateSubscription(); // TODO: implement mthd
        }

        protected Order ConvertOrder(OrderModel order)
        {
            Order qcOrder;

            var symbol = order.OrderLegCollections[0].Instrument.Symbol;// _symbolMapper.GetLeanSymbol(order.Class == TradierOrderClass.Option ? order.OptionSymbol : order.Symbol);
            var quantity = ConvertQuantity(order.Quantity, order.OrderLegCollections[0].InstructionType.ToEnum<InstructionType>());
            var time = Time.ParseDate(order.EnteredTime);

            switch (order.OrderType.ToEnum<Models.OrderType>())
            {
                case Models.OrderType.Market:
                    qcOrder = new MarketOrder(symbol, quantity, time);
                    break;
                case Models.OrderType.Limit:
                    qcOrder = new LimitOrder(symbol, quantity, order.Price, time);
                    break;
                //case Domain.Enums.OrderType.Stop:
                //    qcOrder = new StopMarketOrder(symbol, quantity, order..StopPrice, time);
                //    break;

                //case Domain.Enums.OrderType.StopLimit:
                //    qcOrder = new StopLimitOrder(symbol, quantity, GetOrder(order.Id).StopPrice, order.Price, time);
                //    break;

                //case TradierOrderType.Credit:
                //case TradierOrderType.Debit:
                //case TradierOrderType.Even:
                default:
                    throw new NotImplementedException("The Tradier order type " + order.OrderType + " is not implemented.");
            }

            qcOrder.Status = ConvertStatus(order.Status.ToEnum<OrderStatusType>());
            qcOrder.BrokerId.Add(order.OrderId.ToStringInvariant());
            return qcOrder;
        }

        protected int ConvertQuantity(decimal quantity, InstructionType instructionType)
        {
            switch (instructionType)
            {
                case InstructionType.Buy:
                case InstructionType.BuyToCover:
                case InstructionType.BuyToClose:
                case InstructionType.BuyToOpen:
                    return (int)quantity;

                case InstructionType.SellToClose:
                case InstructionType.Sell:
                case InstructionType.SellToOpen:
                    return -(int)quantity;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected OrderStatus ConvertStatus(OrderStatusType status)
        {
            switch (status)
            {
                case OrderStatusType.Filled:
                    return OrderStatus.Filled;

                case OrderStatusType.Canceled:
                    return OrderStatus.Canceled;

                case OrderStatusType.PendingActivation:
                    return OrderStatus.Submitted;

                case OrderStatusType.Expired:
                case OrderStatusType.Rejected:
                    return OrderStatus.Invalid;

                case OrderStatusType.Queued:
                    return OrderStatus.New;

                case OrderStatusType.Working:
                    return OrderStatus.PartiallyFilled;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
