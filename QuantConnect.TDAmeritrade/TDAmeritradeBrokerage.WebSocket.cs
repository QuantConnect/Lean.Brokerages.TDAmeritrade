using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Data;
using QuantConnect.Logging;
using QuantConnect.Packets;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Web;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        private int _counter;
        private SemaphoreSlim _slim = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<string, DefaultOrderBook> _subscribedTickers = new ConcurrentDictionary<string, DefaultOrderBook>();

        /// <summary>
        /// Returns true if we're currently connected to the broker
        /// </summary>
        public override bool IsConnected => WebSocket.IsOpen;

        public void SetJob(LiveNodePacket job)
        {
            if (!IsConnected)
            {
                Connect();
            }
        }

        public override void Disconnect()
        {
            if (WebSocket != null && WebSocket.IsOpen)
            {
                WebSocket.Close();
            }
        }

        protected override bool Subscribe(IEnumerable<Symbol> symbols)
        {
            var symbolsAdded = false;

            foreach (var symbol in symbols)
            {
                if (!symbol.Value.Contains("universe", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_subscribedTickers.ContainsKey(symbol.Value))
                    {
                        _subscribedTickers.TryAdd(symbol.Value, new DefaultOrderBook(symbol));
                        symbolsAdded = true;
                    }
                }
            }

            if (symbolsAdded)
            {
                SubscribeToLevelOne(_subscribedTickers.Keys.Select(x => x).ToArray());
            }

            return true;
        }

        public IEnumerator<BaseData> Subscribe(SubscriptionDataConfig dataConfig, EventHandler newDataAvailableHandler)
        {
            var enumerator = _aggregator.Add(dataConfig, newDataAvailableHandler);
            SubscriptionManager.Subscribe(dataConfig);

            return enumerator;
        }

        private bool Unsubscribe(IEnumerable<Symbol> symbols)
        {
            var symbolsRemoved = false;
            List<string> removedSymbols = new(); 
            foreach (var symbol in symbols)
            {
                if (!symbol.IsCanonical())
                {
                    if (_subscribedTickers.ContainsKey(symbol.Value))
                    {
                        _subscribedTickers.TryRemove(symbol.Value, out var removedSymbol);
                        removedSymbols.Add(symbol.Value);
                        symbolsRemoved = true;
                    }
                }
            }

            if (symbolsRemoved)
            {
                UnSubscribeToLevelOne(removedSymbols.ToArray());
            }

            return true;
        }

        public void Unsubscribe(SubscriptionDataConfig dataConfig)
        {
            SubscriptionManager.Unsubscribe(dataConfig);
            _aggregator.Remove(dataConfig);
        }

        /// <summary>
        /// Handles websocket received messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void OnMessage(object sender, WebSocketMessage webSocketMessage)
        {
            var message = (WebSocketClientWrapper.TextMessage)webSocketMessage.Data;
            var token = JToken.Parse(message.Message);

            if (token.Type == JTokenType.Null && token is JObject)
            {
                Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error, Token is {token}");
                return;
            }
            
            var tokenRootName = ((JObject)token).Properties().First().Name;

            switch (tokenRootName)
            {
                case "response":
                    HandleResponseData(token["response"][0]);
                    break;
                case "notify":
                    HandleNotify(token["notify"][0]);
                    break;
                case "snapshot": // Snapshot – Snapshot of market data
                    break;
                case "data":
                    HandleStreamingData(token["data"][0]);
                    break;
            }
        }

        public void Login()
        {
            if (WebSocket.IsOpen)
            {
                var userPrincipals = GetUserPrincipals();

                var tokenTimeStampAsDateObj = DateTime.Parse(userPrincipals.StreamerInfo.TokenTimestamp).ToUniversalTime();
                var tokenTimeStampAsMs = Time.DateTimeToUnixTimeStampMilliseconds(tokenTimeStampAsDateObj);

                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

                queryString.Add("userid", userPrincipals.Accounts[0].AccountId);
                queryString.Add("company", userPrincipals.Accounts[0].Company);
                queryString.Add("segment", userPrincipals.Accounts[0].Segment);
                queryString.Add("cddomain", userPrincipals.Accounts[0].AccountCdDomainId);

                queryString.Add("token", userPrincipals.StreamerInfo.Token);
                queryString.Add("usergroup", userPrincipals.StreamerInfo.UserGroup);
                queryString.Add("accessLevel", userPrincipals.StreamerInfo.AccessLevel);
                queryString.Add("appId", userPrincipals.StreamerInfo.AppId);
                queryString.Add("acl", userPrincipals.StreamerInfo.Acl);

                queryString.Add("timestamp", tokenTimeStampAsMs.ToString());
                queryString.Add("authorized", "Y");

                var credits = queryString.ToString();
                var encoded = HttpUtility.UrlEncode(credits);

                var request = new StreamRequestModelContainer
                {
                    Requests = new StreamRequestModel[]
                    {
                    new StreamRequestModel
                    {
                        Service = "ADMIN",
                        Command = "LOGIN",
                        Requestid = 1, // Interlocked.Increment(ref _counter),
                        Account = userPrincipals.Accounts[0].AccountId,
                        Source = userPrincipals.StreamerInfo.AppId,
                        Parameters = new
                        {
                            token = userPrincipals.StreamerInfo.Token,
                            version = "1.0",
                            credential = encoded,
                        }
                    }
                    }
                };

                WebSocket.Send(JsonConvert.SerializeObject(request));
            }
        }

        public void LogOut()
        {
            var userPrincipals = GetUserPrincipals();

            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
    {
                    new StreamRequestModel
                    {
                        Service = "ADMIN",
                        Command = "LOGOUT",
                        Requestid = Interlocked.Increment(ref _counter),
                        Account = userPrincipals.Accounts[0].AccountId,
                        Source = userPrincipals.StreamerInfo.AppId,
                        Parameters = new { }
                    }
    }
            };
            WebSocket.Send(JsonConvert.SerializeObject(request));
        }

        private void SubscribeToLevelOne(params string[] symbols)
        {
            var userPrincipals = GetUserPrincipals();

            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
{
                    new StreamRequestModel
                    {
                        Service = "QUOTE",
                        Command = "SUBS",
                        Requestid = Interlocked.Increment(ref _counter),
                        Account = userPrincipals.Accounts[0].AccountId,
                        Source = userPrincipals.StreamerInfo.AppId,
                        Parameters = new
                        {
                            keys = $"{string.Join(",", symbols)}",
                            fields = "0,1,2,3,4,5,6,7,8"
                            #region Description Fields
                            /* 0 - Ticker symbol in upper case.
                             * 1 - Bid Price, Current Best Bid Price
                             * 2 - Ask Price, Current Best Ask Price
                             * 3 - Last Price, Price at which the last trade was matched
                             * 4 - Bid Size, Number of shares for bid
                             * 5 - Ask Size, Number of shares for ask
                             * 6 - Ask ID, Exchange with the best ask
                             * 7 - Bid ID, Exchange with the best bid
                             * 8 - Total Volume, Aggregated shares traded throughout the day, including pre/post market hours.
                             */
                            #endregion
                        }
                    }
}
            };

            WebSocket.Send(JsonConvert.SerializeObject(request));
        }

        private void UnSubscribeToLevelOne(params string[] symbols)
        {
            var userPrincipals = GetUserPrincipals();

            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
{
                    new StreamRequestModel
                    {
                        Service = "QUOTE",
                        Command = "UNSUBS",
                        Requestid = Interlocked.Increment(ref _counter),
                        Account = userPrincipals.Accounts[0].AccountId,
                        Source = userPrincipals.StreamerInfo.AppId,
                        Parameters = new
                        {
                            keys = $"{string.Join(",", symbols)}"
                        }
                    }
}
            };

            WebSocket.Send(JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// Handle Streaming market data
        /// </summary>
        /// <param name="token">Json</param>
        private void HandleStreamingData(JToken token)
        {
            switch (token["service"].ToString())
            {
                case "QUOTE":
                    ParseQuoteLevelOneData(token["content"]);
                    break;
            }
        }

        /// <summary>
        /// Handle Response from streaming – Response to a request
        /// </summary>
        /// <param name="token">Json</param>
        private void HandleResponseData(JToken token)
        {
            switch (token["command"].ToString())
            {
                case "LOGIN":
                    if (token["content"]["code"].Value<int>() != 0)
                        Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error:HandleResponseData, Login: {token["content"]["msg"]}");
                    break;
                case "LOGOUT":
                    Log.Trace($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleResponseData, Logout: {token["content"]["msg"]}");
                    break;
                case "SUBS":
                    Log.Trace($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleResponseData, Subscribes: {token["content"]["msg"]}");
                    break;
            }
        }

        /// <summary>
        /// Handle Notify response from streaming  – Notification of heartbeats
        /// </summary>
        /// <param name="token">Json</param>
        private void HandleNotify(JToken token)
        {
            var notifyRootName = ((JObject)token).Properties().First().Name;

            switch (notifyRootName)
            {
                case "heartbeat":
                    break;
                case "service":
                    if (token["content"]["code"].Value<int>() == 30)
                        Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error:HandleNotify: {token["content"]["msg"]}");
                    break;
            }
        }

        private void ParseQuoteLevelOneData(JToken content)
        {
            foreach (var symbol in content)
            {
                var symbolOrderBook = _subscribedTickers[symbol["key"].ToString()];

                symbolOrderBook.UpdateBidRow(symbol["1"].Value<decimal>(), symbol["4"].Value<decimal>());
                symbolOrderBook.UpdateAskRow(symbol["2"].Value<decimal>(), symbol["5"].Value<decimal>());
            }
        }
    }
}
