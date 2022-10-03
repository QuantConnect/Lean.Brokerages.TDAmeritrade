using Newtonsoft.Json;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels;
using RestSharp;

namespace QuantConnect.TDAmeritrade.Application
{
    [BrokerageFactory(typeof(TDAmeritrade))]
    public partial class TDAmeritrade : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private readonly string _consumerKey;

        private string _restApiUrl = "https://api.tdameritrade.com/v1/";
        private string _authApiUrl = "https://auth.tdameritrade.com/auth"; // https://developer.tdameritrade.com/content/authentication-faq

        private readonly object _lockAccessCredentials = new object();

        public override bool IsConnected => throw new NotImplementedException();

        private IRestClient _restClient;

        public TDAmeritrade() : base("TD Ameritrade")
        { }

        public TDAmeritrade(string consumerKey) : base("TD Ameritrade")
        {
            _restClient = new RestClient(_restApiUrl);
            _consumerKey = consumerKey;

            //ValidateSubscription(); // Quant Connect api permission
        }

        #region TD Ameritrade client

        private T Execute<T>(RestRequest request, /*TradierApiRequestType type,*/ string rootName = "", int attempts = 0, int max = 10) where T : new()
        {
            var response = default(T);

            var method = "TDAmeritrade.Execute." + request.Resource;
            var parameters = request.Parameters.Select(x => x.Name + ": " + x.Value);

            if (attempts != 0)
            {
                Log.Trace(method + "(): Begin attempt " + attempts);
            }

            lock (_lockAccessCredentials)
            {
                //Wait for the API rate limiting
                //_rateLimitNextRequest[type].WaitToProceed();

                //Send the request:
                var raw = _restClient.Execute(request);
                //_previousResponseRaw = raw.Content;

                if (!raw.IsSuccessful)
                {
                    // fault errors on authentication
                    if (raw.Content.Contains("\"fault\""))
                    {
                        var fault = JsonConvert.DeserializeObject(raw.Content); // TODO: Develop Deserialzie model
                        OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "TDAmeritradeFault", "Error Detail from object"));

                        return default(T);
                    }

                    // this happens when we try to cancel a filled or cancelled order
                    //if (raw.Content.Contains("order already in finalized state:"))
                    //{
                    //    if (request.Method == Method.DELETE)
                    //    {
                    //        var orderId = raw.ResponseUri.Segments.LastOrDefault() ?? "[unknown]";

                    //        OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "OrderAlreadyFilledOrCancelled",
                    //            "Unable to cancel the order because it has already been filled or cancelled. TradierOrderId: " + orderId
                    //        ));
                    //    }
                    //    return default(T);
                    //}

                    // this happens when a request for historical data should return an empty response
                    //if (type == TradierApiRequestType.Data && rootName == "series")
                    //{
                    //    return new T();
                    //}

                    Log.Error($"{method}(2): Parameters: {string.Join(",", parameters)} Response: {raw.Content}");
                    if (attempts++ < max)
                    {
                        Log.Trace(method + "(2): Attempting again...");
                        // this will retry on time outs and other transport exception
                        Thread.Sleep(3000);
                        //return Execute<T>(request, type, rootName, attempts, max);
                        return Execute<T>(request, rootName, attempts, max);
                    }
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, raw.StatusCode.ToStringInvariant(), raw.Content));

                    return default(T);
                }

                try
                {
                    if (!string.IsNullOrEmpty(rootName))
                    {
                        if (TryDeserializeRemoveRoot(raw.Content, rootName, out response))
                        {
                            // if we are able to successfully deserialize the rootName, even if null, return it. For example if there is no historical data
                            // tradier will just return success response with null value in 'rootName' and we don't want to retry & sleep because of it
                            return response;
                        }
                    }
                    else
                    {
                        response = JsonConvert.DeserializeObject<T>(raw.Content);
                    }
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
                        //return Execute<T>(request, type, rootName, attempts, max);
                        return Execute<T>(request, rootName, attempts, max);
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
                    return Execute<T>(request, rootName, attempts, max);
                }

                Log.Trace(method + "(4): Parameters: " + string.Join(",", parameters));
                Log.Error(method + "(4): null response: raw response: " + "_previousresponseraw");
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "NullResponse", "_previousResponseRaw"));
            }

            return response;
        }

        public InstrumentModel GetInstrumentByCUSIP(string cusip)
        {
            var request = new RestRequest($"instruments/{cusip}", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);
            request.AddQueryParameter("projection", "fundamental");

            var instrumentResponse = Execute<List<InstrumentModel>>(request);
            return instrumentResponse.First();
        }

        public InstrumentModel GetSearchInstruments(string symbol, string projection = "symbol-search")
        {
            var request = new RestRequest("instruments", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);
            request.AddQueryParameter("symbol", symbol);
            request.AddQueryParameter("projection", projection);

            var instrumentResponse = Execute<InstrumentModel>(request, symbol);
            return instrumentResponse;
        }

        private bool TryDeserializeRemoveRoot<T>(string json, string rootName, out T obj)
        {
            obj = default;
            var success = false;

            try
            {
                //Dynamic deserialization:
                dynamic dynDeserialized = JsonConvert.DeserializeObject(json);
                obj = JsonConvert.DeserializeObject<T>(dynDeserialized[rootName].ToString());

                // if we arrieved here without exploding it's a success even if obj is null, because that's what we got back
                success = true;
            }
            catch (Exception err)
            {
                Log.Error(err, "RootName: " + rootName);
            }

            return success;
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
