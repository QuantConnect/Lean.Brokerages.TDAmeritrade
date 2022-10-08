using Newtonsoft.Json;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.TDAmeritrade.Domain.Enums;
using RestSharp;

namespace QuantConnect.TDAmeritrade.Application
{
    [BrokerageFactory(typeof(TDAmeritrade))]
    public partial class TDAmeritrade : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private string _accessToken;
        private readonly string _consumerKey;
        private readonly string _refreshToken;
        private readonly string _callbackUrl;
        private readonly string _codeFromUrl;

        private string _restApiUrl = "https://api.tdameritrade.com/v1/";

        private IRestClient _restClient;
        private IAlgorithm _algorithm;

        private readonly object _lockAccessCredentials = new object();

        public override bool IsConnected => throw new NotImplementedException();

        public TDAmeritrade() : base("TD Ameritrade")
        { }

        public TDAmeritrade(
            string consumerKey, 
            string refreshToken, 
            string callbackUrl, 
            string codeFromUrl, 
            IAlgorithm algorithm) : base("TD Ameritrade")
        {
            _restClient = new RestClient(_restApiUrl);
            _consumerKey = consumerKey;
            _refreshToken = refreshToken;
            _callbackUrl = callbackUrl;
            _codeFromUrl = codeFromUrl;
            _algorithm = algorithm;

            //ValidateSubscription(); // Quant Connect api permission
        }

        #region TD Ameritrade client

        private string Execute(RestRequest request, string rootName = "", int attempts = 0, int max = 10)
        {
            if(string.IsNullOrEmpty(_accessToken))
                Task.Run(() => PostAccessToken(GrantType.RefreshToken, string.Empty)).GetAwaiter().GetResult();

            request.AddHeader("Authorization", _accessToken);

            string response = null;

            var method = "TDAmeritrade.Execute." + request.Resource;
            var parameters = request.Parameters.Select(x => x.Name + ": " + x.Value);

            if (attempts != 0)
            {
                Log.Trace(method + "(): Begin attempt " + attempts);
            }

            lock (_lockAccessCredentials)
            {
                var raw = _restClient.Execute(request);

                if (!raw.IsSuccessful)
                {
                    // fault errors on authentication
                    if (raw.Content.Contains("\"fault\""))
                    {
                        var fault = JsonConvert.DeserializeObject(raw.Content); // TODO: Develop Deserialzie model
                        OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "TDAmeritradeFault", "Error Detail from object"));

                        return string.Empty;
                    }

                    Log.Error($"{method}(2): Parameters: {string.Join(",", parameters)} Response: {raw.Content}");

                    if (attempts++ < max)
                    {
                        Log.Trace(method + "(2): Attempting again...");
                        // this will retry on time outs and other transport exception
                        Thread.Sleep(3000);
                        return Execute(request, rootName, attempts, max);
                    }
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, raw.StatusCode.ToStringInvariant(), raw.Content));

                    return string.Empty;
                }

                try
                {
                    if (!string.IsNullOrEmpty(raw.Content))
                        return raw.Content;

                    response = null;
                }
                catch (Exception e)
                {
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "JsonError", $"Error deserializing message: {raw.Content} Error: {e.Message}"));
                }

                if (raw.ErrorException != null)
                {
                    if (attempts++ < max)
                    {
                        Log.Trace(method + "(3): Attempting again...");
                        // this will retry on time outs and other transport exception
                        Thread.Sleep(3000);
                        return Execute(request, rootName, attempts, max);
                    }

                    Log.Trace(method + "(3): Parameters: " + string.Join(",", parameters));
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, raw.ErrorException.GetType().Name, raw.ErrorException.ToString()));

                    const string message = "Error retrieving response.  Check inner details for more info.";
                    throw new ApplicationException(message, raw.ErrorException);
                }
            }

            if (response == null)
            {
                if (attempts++ < max)
                {
                    Log.Trace(method + "(4): Attempting again...");
                    // this will retry on time outs and other transport exception
                    Thread.Sleep(3000);
                    //return Execute<T>(request, type, rootName, attempts, max);
                    return Execute(request, rootName, attempts, max);
                }

                Log.Trace(method + "(4): Parameters: " + string.Join(",", parameters));
                Log.Error(method + "(4): null response: raw response: " + "_previousresponseraw");
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "NullResponse", "_previousResponseRaw"));
            }

            return response;
        }

        private T Execute<T>(RestRequest request, string rootName = "", int attempts = 0, int max = 10) where T : new()
        {
            var response = default(T);

            var raw = Execute(request, rootName, attempts, max);

            if (raw == null)
                return response;

            try
            {
                if (!string.IsNullOrEmpty(rootName))
                {
                    if (TryDeserializeRemoveRoot(raw, rootName, out response))
                    {
                        return response;
                    }
                }
                else
                {
                    response = JsonConvert.DeserializeObject<T>(raw);
                }
            }
            catch (Exception e)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "JsonError", $"Error deserializing message: {raw} Error: {e.Message}"));
            }

            return response;
        }

        protected override void OnMessage(object sender, WebSocketMessage e)
        {
            throw new NotImplementedException();
        }

        protected override bool Subscribe(IEnumerable<Symbol> symbols)
        {
            throw new NotImplementedException();
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

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override List<Order> GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        public override List<Holding> GetAccountHoldings()
        {
            throw new NotImplementedException();
        }

        public override List<CashAmount> GetCashBalance()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<BaseData> Subscribe(SubscriptionDataConfig dataConfig, EventHandler newDataAvailableHandler)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(SubscriptionDataConfig dataConfig)
        {
            throw new NotImplementedException();
        }

        public void SetJob(LiveNodePacket job)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
