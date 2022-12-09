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
using QuantConnect.Packets;
using System.Collections.Concurrent;
using System.Web;
using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        private int _counter;
        private readonly ConcurrentDictionary<Symbol, DefaultOrderBook> _orderBooks = new ConcurrentDictionary<Symbol, DefaultOrderBook>();

        /// <summary>
        /// We're caching orders to increase speed of getting info about ones
        /// </summary>
        private Dictionary<string, OrderModel> _cachedOrdersFromWebSocket = new Dictionary<string, OrderModel>();

        private Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>()
        {
            { typeof(OrderCancelRequestMessage), new XmlSerializer(typeof(OrderCancelRequestMessage))},
            { typeof(OrderEntryRequestMessage), new XmlSerializer(typeof(OrderEntryRequestMessage))},
            { typeof(OrderFillMessage), new XmlSerializer(typeof(OrderFillMessage))},
            { typeof(OrderCancelReplaceRequestMessage), new XmlSerializer(typeof(OrderCancelReplaceRequestMessage))}
        };

        private object _tickLocker = new object();

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
                WebSocket.Close();
            }
        }

        protected override bool Subscribe(IEnumerable<Symbol> symbols)
        {
            var pendingSymbols = new List<string>();

            foreach (var symbol in symbols)
            {
                if (!_orderBooks.ContainsKey(symbol))
                {
                    _orderBooks[symbol] = CreateOrderBookWithEventBestBidAskUpdate(symbol, OnBestBidAskUpdated);
                    var brokerageSymbol = _symbolMapper.GetBrokerageWebsocketSymbol(symbol);
                    pendingSymbols.Add(brokerageSymbol);
                }
            }

            if (pendingSymbols.Any())
            {
                SubscribeToLevelOne(pendingSymbols.ToArray());
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

        public void Login()
        {
            if (WebSocket.IsOpen && !string.IsNullOrEmpty(_refreshToken))
            {
                var userPrincipals = GetUserPrincipals();

                var tokenTimeStampAsDateObj = DateTime.Parse(userPrincipals.StreamerInfo.TokenTimestamp).ToUniversalTime();
                var tokenTimeStampAsMs = Time.DateTimeToUnixTimeStampMilliseconds(tokenTimeStampAsDateObj);

                var queryString = HttpUtility.ParseQueryString(string.Empty);

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
                        var userPrincipals = GetUserPrincipals();
                        SubscribeToAccountActivity(userPrincipals, userPrincipals.StreamerSubscriptionKeys.Keys[0].Key);
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

                DefaultOrderBook symbolOrderBook;
                // After Unsubscribe, we haven't gotten response already, but update will come in this chanel.
                if (!_orderBooks.TryGetValue(symbolLean, out symbolOrderBook))
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
                    var tradeTime = (Time.UnixTimeStampToDateTime(symbol.TradeTime));
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
                Time = DateTime.UtcNow,
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

        private void ParseAccountActivity(JToken content)
        {
            var accountActivityData = content.ToObject<List<AccountActivityResponseModel>>()?.First();

            if (!accountActivityData.HasValue)
            {
                return;
            }

            switch (accountActivityData.Value.MessageType)
            {
                case "SUBSCRIBED":
                    Log.Debug($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: subscribed successfully, Description: {accountActivityData.Value.MessageData}");
                    break;
                case "ERROR":
                    Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: not subscribed, Description: {accountActivityData.Value.MessageData}");
                    break;
                case "OrderCancelRequest": // A request to cancel an order has been received
                    var candelOrder = DeserializeXMLExecutionResponse<OrderCancelRequestMessage>(accountActivityData.Value.MessageData);
                    HandleOrderCancelRequest(candelOrder);
                    break;
                case "OrderCancelReplaceRequest": // A request to modify an order (Cancel/Replace) has been received
                    var cancelReplaceOrder = DeserializeXMLExecutionResponse<OrderCancelReplaceRequestMessage>(accountActivityData.Value.MessageData);
                    HandleOrderCancelReplaceRequest(cancelReplaceOrder);
                    break;
                case "OrderEntryRequest": // A new order has been submitted
                    var newOrder = DeserializeXMLExecutionResponse<OrderEntryRequestMessage>(accountActivityData.Value.MessageData);
                    HandleSubmitNewOrder(newOrder);
                    break;
                case "OrderFill": // An order has been completely filled
                    var orderFill = DeserializeXMLExecutionResponse<OrderFillMessage>(accountActivityData.Value.MessageData);
                    HandleOrderFill(orderFill);
                    break;
            }
        }

        private void HandleSubmitNewOrder(OrderEntryRequestMessage? order)
        {
            if (order == null)
            {
                return;
            }

            _cachedOrdersFromWebSocket[order.Order.OrderKey.ToStringInvariant()] = new OrderModel()
            {
                AccountId = order.OrderGroupID.AccountKey,
                EnteredTime = order.ActivityTimestamp,
                Duration = order.Order.OrderDuration,
                Editable = true,
                OrderId = order.Order.OrderKey,
                OrderLegCollections = new List<OrderLegCollectionModel>()
                {
                    new OrderLegCollectionModel()
                    {
                        InstructionType = order.Order.OrderInstructions.ToStringInvariant(),
                        Instrument = new InstrumentModel()
                        {
                            AssetType = order.Order.Security.SecurityType,
                            Cusip = order.Order.Security.CUSIP,
                            Symbol = order.Order.Security.Symbol
                        },
                        Quantity = order.Order.OriginalQuantity
                    }
                },
                Quantity = order.Order.OriginalQuantity,
                OrderType = order.Order.OrderType.ToString(),
                Price = (order.Order.OrderPricing as OrderEntryRequestMessageOrderOrderPricingLimit)?.Limit ??
                        (order.Order.OrderPricing as OrderEntryRequestMessageOrderOrderPricingStopLimit)?.Limit ?? 0m,
                Status = OrderStatusType.Working,
                StopPrice = (order.Order.OrderPricing as OrderEntryRequestMessageOrderOrderPricingStopMarket)?.Stop ??
                            (order.Order.OrderPricing as OrderEntryRequestMessageOrderOrderPricingStopLimit)?.Stop ?? 0m,
            };

            // Reset Event for PlaceOrder mthd()
            _onOrderWebSocketResponseEvent.Set();
        }

        private void HandleOrderFill(OrderFillMessage? orderFillResponse)
        {
            if (orderFillResponse == null)
            {
                return;
            }

            var brokerageOrderKey = orderFillResponse.Order.OrderKey.ToStringInvariant();
            OrderModel cashedOrder = TryGetCashedOrder(brokerageOrderKey);

            if (cashedOrder == null)
            {
                Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleOrderFill(): Unable to locate order with BrokerageId: {brokerageOrderKey}");
                return;
            }

            cashedOrder.Status = OrderStatusType.Filled;
            cashedOrder.CloseTime = orderFillResponse.ExecutionInformation.Timestamp.ToStringInvariant();
            cashedOrder.Price = orderFillResponse.ExecutionInformation.ExecutionPrice;
            cashedOrder.Quantity = orderFillResponse.ExecutionInformation.Quantity;
            _cachedOrdersFromWebSocket[brokerageOrderKey] = cashedOrder;
        }

        private void HandleOrderCancelRequest(OrderCancelRequestMessage? orderCancelResponse)
        {
            if (orderCancelResponse == null)
            {
                return;
            }

            var brokerageOrderKey = orderCancelResponse.Order.OrderKey.ToStringInvariant();
            OrderModel cashedOrder = TryGetCashedOrder(brokerageOrderKey);

            if (cashedOrder == null)
            {
                Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleOrderCancelRequest(): Unable to locate order with BrokerageId: {brokerageOrderKey}");
                return;
            }

            cashedOrder.Status = OrderStatusType.Canceled;
            _cachedOrdersFromWebSocket[brokerageOrderKey] = cashedOrder;

            // Reset Event for CancelOrder mthd()
            _onOrderWebSocketResponseEvent.Set();
        }

        private void HandleOrderCancelReplaceRequest(OrderCancelReplaceRequestMessage? orderCancelReplaceMessage)
        {
            if (orderCancelReplaceMessage == null)
            {
                return;
            }

            var oldBrokerageOrderKey = orderCancelReplaceMessage.OriginalOrderId.ToStringInvariant();
            OrderModel cashedOrder = TryGetCashedOrder(oldBrokerageOrderKey);

            if (cashedOrder == null)
            {
                Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:HandleOrderFill(): Unable to locate order with BrokerageId: {oldBrokerageOrderKey}");
                return;
            }

            var newBrokerageOrderKey = orderCancelReplaceMessage.Order.OrderKey;

            cashedOrder.Status = OrderStatusType.Replaced;
            cashedOrder.OrderId = newBrokerageOrderKey;

            cashedOrder.Price = (orderCancelReplaceMessage.Order.OrderPricing as OrderCancelReplaceRequestMessageOrderOrderPricingLimit)?.Limit ??
                                (orderCancelReplaceMessage.Order.OrderPricing as OrderCancelReplaceRequestMessageOrderOrderPricingStopLimit)?.Limit ?? 0m;
            cashedOrder.StopPrice = (orderCancelReplaceMessage.Order.OrderPricing as OrderCancelReplaceRequestMessageOrderOrderPricingStopMarket)?.Stop ??
                                    (orderCancelReplaceMessage.Order.OrderPricing as OrderCancelReplaceRequestMessageOrderOrderPricingStopLimit)?.Stop ?? 0m;
            cashedOrder.Quantity = orderCancelReplaceMessage.PendingCancelQuantity;

            // add new order to cache collection
            _cachedOrdersFromWebSocket[newBrokerageOrderKey.ToStringInvariant()] = cashedOrder;
            // remove order from cache collection
            _cachedOrdersFromWebSocket.Remove(oldBrokerageOrderKey);

            // Reset Event for UpdateOrder mthd()
            _onOrderWebSocketResponseEvent.Set();
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

        private OrderModel? TryGetCashedOrder(string orderKey)
            => _cachedOrdersFromWebSocket.TryGetValue(orderKey, out OrderModel orderModel) ? orderModel : null;

        private DefaultOrderBook CreateOrderBookWithEventBestBidAskUpdate(Symbol symbol, EventHandler<BestBidAskUpdatedEventArgs> updateEvent)
        {
            var orderBook = new DefaultOrderBook(symbol);
            orderBook.BestBidAskUpdated += updateEvent;
            return orderBook;
        }
    }
}
