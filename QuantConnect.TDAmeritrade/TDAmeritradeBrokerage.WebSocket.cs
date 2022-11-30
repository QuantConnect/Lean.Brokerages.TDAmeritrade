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
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Packets;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Web;
using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        private int _counter;
        private readonly ConcurrentDictionary<Symbol, DefaultOrderBook> _subscribedTickers = new ConcurrentDictionary<Symbol, DefaultOrderBook>();

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
                LogOut();
            }
        }

        protected override bool Subscribe(IEnumerable<Symbol> symbols)
        {
            var symbolsAdded = false;

            foreach (var symbol in symbols)
            {
                if (!symbol.Value.Contains("universe", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_subscribedTickers.ContainsKey(symbol))
                    {
                        _subscribedTickers.TryAdd(symbol, new DefaultOrderBook(symbol));
                        symbolsAdded = true;
                        _symbolMapper.GetWebsocketSymbol(symbol);
                    }
                }
            }

            if (symbolsAdded)
            {
                SubscribeToLevelOne(_subscribedTickers.Keys.Select(x => x.Value).ToArray());
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
                    if (_subscribedTickers.ContainsKey(symbol))
                    {
                        _subscribedTickers.TryRemove(symbol, out var removedSymbol);
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
                        Requestid = Interlocked.Increment(ref _counter),
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
                
                // After login, we need to subscribe to account's Trade activity chanel
                SubscribeToAccountActivity(userPrincipals, userPrincipals.StreamerSubscriptionKeys.Keys[0].Key);
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
                            fields = "0,1,2,3,4,5,6,7,8,9,10,16"
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
                             * 9 - Last Size, Number of shares traded with last trade
                             * 10 - Trade Time, Trade time of the last trade
                             * 16 - Exchange ID, Primary "listing" Exchange
                             */
                            #endregion
                        }
                    }
}
            };

            WebSocket.Send(JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// This service is used to request streaming updates for one or more accounts associated with the logged in User ID.  
        /// Common usage would involve issuing the OrderStatus API request to get all transactions for an account, and subscribing to 
        /// ACCT_ACTIVITY to get any updates. 
        /// </summary>
        private void SubscribeToAccountActivity(UserPrincipalsModel userPrincipals, string streamSubscriptionKey)
        {
            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
                {
                    new StreamRequestModel
                    {
                        Service = "ACCT_ACTIVITY",
                        Command = "SUBS",
                        Requestid = Interlocked.Increment(ref _counter),
                        Account = userPrincipals.Accounts[0].AccountId,
                        Source = userPrincipals.StreamerInfo.AppId,
                        Parameters = new
                        {
                            keys = $"{streamSubscriptionKey}",
                            fields = "0,1,2,3"
                            #region Description Fields
                            /* 0 - Subscription Key
                             * 1 - Account #
                             * 2 - Message Type
                             * 3 - Message Data
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
                case "ACCT_ACTIVITY":
                    ParseAccountActivity(token["content"]);
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
                    {
                        Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error:HandleResponseData, Login: {token["content"]["msg"]}");
                    }
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
            var levelOneData = content.ToObject<List<LevelOneResponseModel>>() ?? new List<LevelOneResponseModel>(0);
            foreach (var symbol in levelOneData)
            {
                var symbolLean = _symbolMapper.GetSymbolFromWebsocket(symbol.Symbol);

                DefaultOrderBook symbolOrderBook;
                if (!_subscribedTickers.TryGetValue(symbolLean, out symbolOrderBook))
                    continue;

                if (symbol.BidPrice > 0)
                    symbolOrderBook.UpdateBidRow(symbol.BidPrice, symbol.BidSize);

                if (symbol.AskPrice > 0)
                    symbolOrderBook.UpdateAskRow(symbol.AskPrice, symbol.AskSize);

                if (symbol.LastPrice > 0 && symbol.LastSize > 0)
                {
                    var tradeTime = (Time.UnixTimeStampToDateTime(symbol.TradeTime));
                    var exchange = ConvertIDExchangeToFullName(symbol.ExchangeID);
                    var trade = new Tick(tradeTime, symbolLean, "", exchange, symbol.LastSize, symbol.LastPrice);
                    _aggregator.Update(trade);
                }
            }
        }

        private void ParseAccountActivity(JToken content)
        {
            var accountActivityData = content.ToObject<List<AccountActivityResponseModel>>()?.First();

            if (!accountActivityData.HasValue)
                return;

            switch (accountActivityData.Value.MessageType)
            {
                case "SUBSCRIBED":
                    Log.Trace($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: subscribed successfully, Description: {accountActivityData.Value.MessageData}");
                    break;
                case "ERROR":
                    Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: not subscribed, Description: {accountActivityData.Value.MessageData}");
                    break;
                case "BrokenTrade": // After an order was filled, the trade is reversed or "Broken" and the order is changed to Canceled.
                    break;
                case "ManualExecution": // The order is manually entered (and filled) by the broker.  Usually due to some system issue.
                    break;
                case "OrderActivation": // A Stop order has been Activated
                    break;
                case "OrderCancelReplaceRequest": // A request to modify an order (Cancel/Replace) has been received (You will also get a UROUT for the original order)
                    break;
                case "OrderCancelRequest": // A request to cancel an order has been received
                    var candelOrder = DeserializeXMLExecutionResponse<OrderCancelRequestMessage>(accountActivityData.Value.MessageData);
                    HandleOrderCancelRequest(candelOrder);
                    break;
                case "OrderEntryRequest": // A new order has been submitted
                    var newOrder = DeserializeXMLExecutionResponse<OrderEntryRequestMessage>(accountActivityData.Value.MessageData);
                    HandleSubmitNewOrder(newOrder);
                    break;
                case "OrderFill": // An order has been completely filled
                    var orderFill = DeserializeXMLExecutionResponse<OrderFillMessage>(accountActivityData.Value.MessageData);
                    HandleOrderFill(orderFill);
                    break;
                case "OrderPartialFill": // An order has been partial filled
                    break;
                case "OrderRejection": // An order was rejected
                    break;
                case "TooLateToCancel": // A request to cancel an order has been received but the order cannot be canceled either because it was already canceled, filled, or for some other reason
                    break;
                case "UROUT": // Indicates "You Are Out" - that the order has been canceled
                    DeserializeXMLExecutionResponse<UROUTMessage>(accountActivityData.Value.MessageData);
                    break;
            }
        }

        private void HandleSubmitNewOrder(OrderEntryRequestMessage? order)
        {
            if(order == null)
            {
                return;
            }

            var qcOrder = _orderProvider.GetOrders(x => x.Status == OrderStatus.None).Last();

            var time = order.Order.OrderEnteredDateTime;

            qcOrder.Status = OrderStatus.Submitted;
            qcOrder.BrokerId.Add(order.Order.OrderKey.ToStringInvariant());
            
            OnOrderEvent(new OrderEvent(qcOrder, time, Orders.Fees.OrderFee.Zero, "TDAmeritrade Order Event SubmitNewOrder") { Status = OrderStatus.Submitted });
            Log.Trace($"Order submitted successfully - OrderId: {order.Order.OrderKey.ToStringInvariant()}");
        }

        private void HandleOrderFill(OrderFillMessage? order)
        {
            if(order == null)
            {
                return;
            }

            OrderStatus orderStatus = OrderStatus.Filled;

            var orderId = order.Order.OrderKey.ToStringInvariant();

            var time = order.ExecutionInformation.Timestamp;

            var orderQC = _orderProvider.GetOrderByBrokerageId(orderId);

            if (orderQC == null)
            {
                Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleOrderFill(): Unable to locate order with BrokerageId: {orderId}");
                return;
            }

            var orderEvent = new OrderEvent(orderQC, time, Orders.Fees.OrderFee.Zero, "TDAmeritradeBrokerage Order Event OrderFill")
            {
                Status = orderStatus,
                FillQuantity = order.ExecutionInformation.Quantity,
                FillPrice = order.ExecutionInformation.ExecutionPrice
            };

            OnOrderEvent(orderEvent);
        }

        private void HandleOrderCancelRequest(OrderCancelRequestMessage? order)
        {
            if(order == null)
            {
                return;
            }

            OrderStatus orderStatus = OrderStatus.Canceled;

            var orderId = order.Order.OrderKey.ToStringInvariant();

            var time = order.LastUpdated;

            var orderQC = _orderProvider.GetOrderByBrokerageId(orderId);

            if (orderQC == null)
            {
                Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleOrderCancelRequest(): Unable to locate order with BrokerageId: {orderId}");
                return;
            }

            var orderEvent = new OrderEvent(orderQC, time, Orders.Fees.OrderFee.Zero, "TDAmeritradeBrokerage Order Event OrderCancel")
            {
                Status = orderStatus,
                FillQuantity = order.PendingCancelQuantity,
            };

            OnOrderEvent(orderEvent);
        }

        private T? DeserializeXMLExecutionResponse<T>(string xml) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (TextReader reader = new StringReader(xml))
            {
                try
                {
                    return (T?)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage: : {ex.Message}");
                }
            }
            return default;
        }

        private string ConvertIDExchangeToFullName(char exchangeID) => exchangeID switch
        {
            'n' => "NYSE",
            'q' => "NASDAQ",
            'p' => "PACIFIC",
            'g' => "AMEX_INDEX",
            'm' => "MUTUAL_FUND",
            '9' => "PINK_SHEET",
            'a' => "AMEX",
            'u' => "OTCBB",
            'x' => "INDICES",
            _ => "unknown"
        };
    }
}
