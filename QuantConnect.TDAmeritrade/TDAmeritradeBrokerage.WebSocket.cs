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
using NodaTime;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using System.Collections.Concurrent;
using System.Web;
using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        /// <summary>
        /// Keep value to next subscribe event on websocket channel
        /// </summary>
        private int _requestIdCounter;

        /// <summary>
        /// Keep and handle actual order book websocket response
        /// </summary>
        private readonly ConcurrentDictionary<Symbol, DefaultOrderBook> _orderBooks = new ConcurrentDictionary<Symbol, DefaultOrderBook>();

        /// <summary>
        /// We're caching submit orders. 
        /// Collection use only in TDAmeritradeBrokerage.PlaceOrder(), to return correct BrokerId in Lean.Order
        /// </summary>
        private ConcurrentQueue<string> _submittedOrderIds = new ConcurrentQueue<string>();

        // exchange time zones by symbol
        private readonly Dictionary<Symbol, DateTimeZone> _symbolExchangeTimeZones = new ();

        /// <summary>
        /// XML serializers instance to parse xml from websocket
        /// </summary>
        private Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>()
        {
            { typeof(OrderCancelRequestMessage), new XmlSerializer(typeof(OrderCancelRequestMessage))},
            { typeof(OrderEntryRequestMessage), new XmlSerializer(typeof(OrderEntryRequestMessage))},
            { typeof(OrderFillMessage), new XmlSerializer(typeof(OrderFillMessage))},
            { typeof(OrderCancelReplaceRequestMessage), new XmlSerializer(typeof(OrderCancelReplaceRequestMessage))},
            { typeof(UROUTMessage), new XmlSerializer(typeof(UROUTMessage))},
            { typeof(OrderRouteMessage), new XmlSerializer(typeof(OrderRouteMessage))}
        };

        /// <summary>
        /// We keep userPrincipals in first request when Login to system
        /// Then we need some param to subscribe\unsubscribe in channels
        /// Also, we decrease amount requests
        /// </summary>
        private UserPrincipalsModel _userPrincipals;

        /// <summary>
        /// Returns true if we're currently connected to the broker
        /// </summary>
        public override bool IsConnected => WebSocket.IsOpen;

        /// <summary>
        /// Sets the job we're subscribing for
        /// </summary>
        /// <param name="job">Job we're subscribing for</param>
        public void SetJob(LiveNodePacket job)
        {
            var consumerKey = job.BrokerageData["tdameritrade-api-key"];
            var accessToken = job.BrokerageData["tdameritrade-access-token"];
            var accountNumber = job.BrokerageData["tdameritrade-account-number"];

            Initialize(
                consumerKey,
                accessToken,
                accountNumber);

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
                WebSocket.Close();
            }
        }

        protected override bool Subscribe(IEnumerable<Symbol> symbols)
        {
            var isSubscribeToNewSymbol = false;

            foreach (var symbol in symbols)
            {
                if (!_orderBooks.ContainsKey(symbol))
                {
                    _orderBooks[symbol] = CreateOrderBookWithEventBestBidAskUpdate(symbol, OnBestBidAskUpdated);
                    var brokerageSymbol = _symbolMapper.GetBrokerageWebsocketSymbol(symbol);
                    isSubscribeToNewSymbol = true;
                }
            }

            if (isSubscribeToNewSymbol)
            {
                SubscribeToLevelOne(_orderBooks.Keys.Select(symbol => _symbolMapper.GetBrokerageWebsocketSymbol(symbol)).ToArray());
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
            var unsubscribeSymbols = new List<string>();
            foreach (var symbol in symbols)
            {
                if (_orderBooks.ContainsKey(symbol))
                {
                    _orderBooks.TryRemove(symbol, out var removedSymbol);
                    unsubscribeSymbols.Add(_symbolMapper.GetBrokerageWebsocketSymbol(symbol));
                }
            }

            if (unsubscribeSymbols.Any())
            {
                UnSubscribeToLevelOne(unsubscribeSymbols.ToArray());
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

        private void Login()
        {
            if (WebSocket.IsOpen && !string.IsNullOrEmpty(_refreshToken))
            {
                _userPrincipals = GetUserPrincipals();

                var tokenTimeStampAsDateObj = DateTime.Parse(_userPrincipals.StreamerInfo.TokenTimestamp).ToUniversalTime();
                var tokenTimeStampAsMs = Time.DateTimeToUnixTimeStampMilliseconds(tokenTimeStampAsDateObj);

                var queryString = HttpUtility.ParseQueryString(string.Empty);

                queryString.Add("userid", _userPrincipals.Accounts[0].AccountId);
                queryString.Add("company", _userPrincipals.Accounts[0].Company);
                queryString.Add("segment", _userPrincipals.Accounts[0].Segment);
                queryString.Add("cddomain", _userPrincipals.Accounts[0].AccountCdDomainId);

                queryString.Add("token", _userPrincipals.StreamerInfo.Token);
                queryString.Add("usergroup", _userPrincipals.StreamerInfo.UserGroup);
                queryString.Add("accessLevel", _userPrincipals.StreamerInfo.AccessLevel);
                queryString.Add("appId", _userPrincipals.StreamerInfo.AppId);
                queryString.Add("acl", _userPrincipals.StreamerInfo.Acl);

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
                        Requestid = Interlocked.Increment(ref _requestIdCounter),
                        Account = _userPrincipals.Accounts[0].AccountId,
                        Source = _userPrincipals.StreamerInfo.AppId,
                        Parameters = new
                        {
                            token = _userPrincipals.StreamerInfo.Token,
                            version = "1.0",
                            credential = encoded,
                        }
                    }
                    }
                };

                WebSocket.Send(JsonConvert.SerializeObject(request));
            }
        }

        private void LogOut()
        {
            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
    {
                    new StreamRequestModel
                    {
                        Service = "ADMIN",
                        Command = "LOGOUT",
                        Requestid = Interlocked.Increment(ref _requestIdCounter),
                        Account = _userPrincipals.Accounts[0].AccountId,
                        Source = _userPrincipals.StreamerInfo.AppId,
                        Parameters = new { }
                    }
    }
            };
            WebSocket.Send(JsonConvert.SerializeObject(request));
        }

        private void SubscribeToLevelOne(params string[] symbols)
        {
            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
{
                    new StreamRequestModel
                    {
                        Service = "QUOTE",
                        Command = "SUBS",
                        Requestid = Interlocked.Increment(ref _requestIdCounter),
                        Account = _userPrincipals.Accounts[0].AccountId,
                        Source = _userPrincipals.StreamerInfo.AppId,
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
                        Requestid = Interlocked.Increment(ref _requestIdCounter),
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
            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
                {
                    new StreamRequestModel
                    {
                        Service = "QUOTE",
                        Command = "UNSUBS",
                        Requestid = Interlocked.Increment(ref _requestIdCounter),
                        Account = _userPrincipals.Accounts[0].AccountId,
                        Source = _userPrincipals.StreamerInfo.AppId,
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
                    else
                    {
                        // After login, we need to subscribe to account's Trade activity chanel
                        SubscribeToAccountActivity(_userPrincipals, _userPrincipals.StreamerSubscriptionKeys.Keys[0].Key);
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
                    HandleNotifyServiceResponse(token["content"]);
                    break;
            }
        }

        private void ParseQuoteLevelOneData(JToken content)
        {
            var levelOneData = content.ToObject<List<LevelOneResponseModel>>() ?? new List<LevelOneResponseModel>(0);
            foreach (var symbol in levelOneData)
            {
                var symbolLean = _symbolMapper.GetLeanSymbolByBrokerageWebsocketSymbol(symbol.Symbol);

                // After Unsubscribe, we haven't gotten response already, but update will come in this chanel.
                if (!_orderBooks.TryGetValue(symbolLean, out var symbolOrderBook))
                {
                    return;
                }

                if (symbol.BidSize == 0)
                {
                    symbolOrderBook.RemoveBidRow(symbol.BidPrice);
                }
                else
                {
                    symbolOrderBook.UpdateBidRow(symbol.BidPrice, symbol.BidSize);
                }

                if (symbol.AskSize == 0)
                {
                    symbolOrderBook.RemoveAskRow(symbol.AskPrice);
                }
                else
                {
                    symbolOrderBook.UpdateAskRow(symbol.AskPrice, symbol.AskSize);
                }

                if (symbol.LastPrice > 0 && symbol.LastSize > 0)
                {
                    var tradeTime = DateTime.UtcNow.ConvertFromUtc(GetSymbolExchange(symbolLean));
                    EmitTradeTick(symbolLean, symbol.LastPrice, symbol.LastSize, tradeTime);
                }
            }
        }

        private void OnBestBidAskUpdated(object? sender, BestBidAskUpdatedEventArgs e)
        {
            EmitQuoteTick(e.Symbol, e.BestBidPrice, e.BestBidSize, e.BestAskPrice, e.BestAskSize);
        }

        /// <summary>
        /// Emits a new quote tick
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="bidPrice">The bid price</param>
        /// <param name="bidSize">The bid size</param>
        /// <param name="askPrice">The ask price</param>
        /// <param name="askSize">The ask price</param>
        private void EmitQuoteTick(Symbol symbol, decimal bidPrice, decimal bidSize, decimal askPrice, decimal askSize)
        {
             _aggregator.Update(new Tick
            {
                AskPrice = askPrice,
                BidPrice = bidPrice,
                Value = (askPrice + bidPrice) / 2m,
                Time = DateTime.UtcNow.ConvertFromUtc(GetSymbolExchange(symbol)),
                Symbol = symbol,
                TickType = TickType.Quote,
                AskSize = askSize,
                BidSize = bidSize
            });
        }

        /// <summary>
        /// Emits a new trade tick from a match message
        /// </summary>
        private void EmitTradeTick(Symbol symbol, decimal price, decimal size, DateTime tradeTime)
        {
            _aggregator.Update(new Tick
            {
                Value = price,
                Time = tradeTime,
                Symbol = symbol,
                TickType = TickType.Trade,
                Quantity = size
            });
        }

        private DateTimeZone GetSymbolExchange(Symbol symbol)
        {
            lock (_symbolExchangeTimeZones)
            {
                if (!_symbolExchangeTimeZones.TryGetValue(symbol, out var exchangeTimeZone))
                {
                    // read the exchange time zone from market-hours-database
                    exchangeTimeZone = MarketHoursDatabase.FromDataFolder().GetExchangeHours(symbol.ID.Market, symbol, symbol.SecurityType).TimeZone;
                    _symbolExchangeTimeZones[symbol] = exchangeTimeZone;
                }
                return exchangeTimeZone;
            }
        }

        private void ParseAccountActivity(JToken content)
        {
            var accountActivityData = content.ToObject<List<AccountActivityResponseModel>>();

            if (accountActivityData == null)
            {
                return;
            }

            foreach(var accountActivity in accountActivityData)
            { 
                switch (accountActivity.MessageType)
                {
                    case "SUBSCRIBED":
                        Log.Debug($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: subscribed successfully, Description: {accountActivity.MessageData}");
                        break;
                    case "ERROR":
                        Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: not subscribed, Description: {accountActivity.MessageData}");
                        break;
                    case "OrderCancelRequest": // A request to cancel an order has been received
                        var candelOrder = DeserializeXMLExecutionResponse<OrderCancelRequestMessage>(accountActivity.MessageData);
                        HandleOrderCancelRequest(candelOrder);
                        break;
                    case "OrderCancelReplaceRequest": // A request to modify an order (Cancel/Replace) has been received
                        var cancelReplaceOrder = DeserializeXMLExecutionResponse<OrderCancelReplaceRequestMessage>(accountActivity.MessageData);
                        HandleOrderCancelReplaceRequest(cancelReplaceOrder);
                        break;
                    case "OrderEntryRequest": // A new order has been submitted
                        var newOrder = DeserializeXMLExecutionResponse<OrderEntryRequestMessage>(accountActivity.MessageData);
                        HandleSubmitNewOrder(newOrder);
                        break;
                    case "OrderFill": // An order has been completely filled
                        var orderFill = DeserializeXMLExecutionResponse<OrderFillMessage>(accountActivity.MessageData);
                        HandleOrderFill(orderFill);
                        break;
                    case "TooLateToCancel":
                        Log.Debug($"TooLateToCancel: {accountActivity.MessageData}");
                        break;
                    case "UROUT": // Indicates "You Are Out" - that the order has been canceled
                        var urout = DeserializeXMLExecutionResponse<UROUTMessage>(accountActivity.MessageData);
                        HandleUroutResponse(urout);
                        break;
                }
            }
        }

        private void HandleSubmitNewOrder(OrderEntryRequestMessage? order)
        {
            if (order == null)
            {
                return;
            }

            // the order add in queue to return first order to PlaceOrder() with correct brokerID
            _submittedOrderIds.Enqueue(order.Order.OrderKey.ToStringInvariant());

            // Reset Event for PlaceOrder mthd()
            _onSumbitOrderWebSocketResponseEvent.Set();
        }

        private void HandleOrderFill(OrderFillMessage? orderFillResponse)
        {
            if (orderFillResponse == null)
            {
                return;
            }

            var brokerageOrderKey = orderFillResponse.Order.OrderKey.ToStringInvariant();

            if (!TryGetLeanOrderById(brokerageOrderKey, out var leanOrder))
            {
                return;
            }

            var fillEvent = new OrderEvent(leanOrder, orderFillResponse.ExecutionInformation.Timestamp, OrderFee.Zero, "TDAmeritradeBrokerage Fill Event")
            {
                // TODO: Same comment as bellow, partial fills/fill price
                Status = OrderStatus.Filled,
                FillPrice = orderFillResponse.ExecutionInformation.ExecutionPrice,
                FillQuantity = orderFillResponse.ExecutionInformation.Quantity
            };
            OnOrderEvent(fillEvent);
        }

        private void HandleOrderCancelRequest(OrderCancelRequestMessage? orderCancelResponse)
        {
            if (orderCancelResponse == null)
            {
                return;
            }

            var brokerageOrderKey = orderCancelResponse.Order.OrderKey.ToStringInvariant();

            if (!TryGetLeanOrderById(brokerageOrderKey, out var leanOrder))
            {
                return;
            }

            OnOrderEvent(new OrderEvent(leanOrder, DateTime.UtcNow, OrderFee.Zero, "TDAmeritradeBrokerage Cancel Event") 
            { Status = OrderStatus.CancelPending });
        }

        private void HandleOrderCancelReplaceRequest(OrderCancelReplaceRequestMessage? orderCancelReplaceMessage)
        {
            if (orderCancelReplaceMessage == null)
            {
                return;
            }

            var oldBrokerageOrderKey = orderCancelReplaceMessage.OriginalOrderId.ToStringInvariant();

            var newBrokerageOrderKey = orderCancelReplaceMessage.Order.OrderKey.ToStringInvariant();

            if (!TryGetLeanOrderById(oldBrokerageOrderKey, out var leanOrder))
            {
                return;
            }

            leanOrder.BrokerId.Remove(oldBrokerageOrderKey);
            leanOrder.BrokerId.Add(newBrokerageOrderKey);

            var updateOrderEvent = new BrokerageOrderIdChangedEvent()
            {
                OrderId = leanOrder.Id,
                BrokerId = leanOrder.BrokerId
            };

            OnOrderIdChangedEvent(updateOrderEvent);

            OnOrderEvent(new OrderEvent(leanOrder, DateTime.UtcNow, OrderFee.Zero) { Status = OrderStatus.UpdateSubmitted });

            // Reset Event for UpdateOrder mthd()
            _onUpdateOrderWebSocketResponseEvent.Set();
        }

        private void HandleUroutResponse(UROUTMessage? uroutMessage)
        {
            if(uroutMessage == null)
            {
                return;
            }

            var key = uroutMessage.Order.OrderKey.ToStringInvariant();
            var leanOrder = _orderProvider.GetOrdersByBrokerageId(key)?.SingleOrDefault();
            if (leanOrder == null)
            {
                Log.Error($"TDAmeritradeBrokerage.WebSocket.HandleUroutResponse(): lean order not found {key}");
                return;
            }

            OnOrderEvent(new OrderEvent(leanOrder, DateTime.UtcNow, OrderFee.Zero, "TDAmeritradeBrokerage Cancel Event")
            { Status = OrderStatus.Canceled });
        }

        private void HandleNotifyServiceResponse(JToken content)
        {
            switch (content["code"].Value<int>())
            {
                case 30:
                    Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error:HandleNotify: {content["msg"]}");
                    break;
                case 12:
                    Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error:HandleNotify: {content["msg"]}");
                    break;
            }
        }

        private T? DeserializeXMLExecutionResponse<T>(string xml) where T : class
        {
            var serializer = _serializers[typeof(T)];

            using (var reader = new StringReader(xml))
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

        private DefaultOrderBook CreateOrderBookWithEventBestBidAskUpdate(Symbol symbol, EventHandler<BestBidAskUpdatedEventArgs> updateEvent)
        {
            var orderBook = new DefaultOrderBook(symbol);
            orderBook.BestBidAskUpdated += updateEvent;
            return orderBook;
        }

        private bool TryGetLeanOrderById(string orderID, out Order leanOrder)
        {
            leanOrder = _orderProvider.GetOrdersByBrokerageId(orderID)?.SingleOrDefault();

            if (leanOrder == null || leanOrder.Status == OrderStatus.Filled)
            {
                if (_onPlaceOrderBrokerageIdResponseEvent.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    // the order was still being processed but now it's ready to be fetched, let's retry
                    _onPlaceOrderBrokerageIdResponseEvent.Reset();
                    return TryGetLeanOrderById(orderID, out leanOrder);
                }
                Log.Error("TDAmeritradeBrokerage.WebSocket.HandleOrderRoute(): Lean order didn't find or one have been filled already.");
                return false;
            }
            return true;
        }

}
}
