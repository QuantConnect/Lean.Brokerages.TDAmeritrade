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
using System.Web;
using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        private int _counter;
        private readonly ConcurrentDictionary<Symbol, DefaultOrderBook> _subscribedTickers = new ConcurrentDictionary<Symbol, DefaultOrderBook>();

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
                var subscribeSymbolArray = _subscribedTickers.Keys.Select(x => x.Value).ToArray();
                SubscribeToLevelOne(subscribeSymbolArray);
                SubscribetToChart(subscribeSymbolArray);
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
            var removedSymbols = new List<string>();
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
                var removeSymbolArray = removedSymbols.ToArray();
                UnSubscribeToLevelOne(removeSymbolArray);
                UnSubscribeToChart(removeSymbolArray);
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
        /// Chart provides  streaming one minute OHLCV (Open/High/Low/Close/Volume) for a one minute period.
        /// </summary>
        /// <remarks>
        /// The one minute bar falls on the 0 second slot (ie. 9:30:00) and includes data from 0 second to 59 seconds.
        /// </remarks>
        /// <example>
        /// For example, a 9:30 bar includes data from 9:30:00 through 9:30:59.
        /// </example>
        /// <param name="symbols"></param>
        private void SubscribetToChart(params string[] symbols)
        {
            var userPrincipals = GetUserPrincipals();

            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
                {
                    new StreamRequestModel
                    {
                        Service = "CHART_EQUITY",
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
                             * 1 - Open Price, Opening price for the minute
                             * 2 - High Price, Highest price for the minute
                             * 3 - Low Price, Chart’s lowest price for the minute
                             * 4 - Close Price, Closing price for the minute
                             * 5 - Volume, Total volume for the minute
                             * 6 - Sequence, Identifies the candle minute
                             * 7 - Chart Time, Milliseconds since Epoch
                             * 8 - Chart Day, Not useful
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

        private void UnSubscribeToChart(params string[] symbols)
        {
            var userPrincipals = GetUserPrincipals();

            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
                {
                    new StreamRequestModel
                    {
                        Service = "CHART_EQUITY",
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
                case "CHART_EQUITY":
                    ParseChartEquityData(token["content"]);
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
                    {
                        Log.Error($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:Error:HandleNotify: {token["content"]["msg"]}");
                    }
                    break;
            }
        }

        private void ParseChartEquityData(JToken content)
        {
            var charts = content.ToObject<List<ChartEquityModel>>() ?? new List<ChartEquityModel>(0);

            foreach (var chart in charts)
            {
                var symbolLean = _symbolMapper.GetSymbolFromWebsocket(chart.Symbol);

                _aggregator.Update(new TradeBar
                {
                    Symbol = symbolLean,
                    Open = chart.OpenPrice,
                    Close = chart.ClosePrice,
                    High = chart.HighPrice,
                    Low = chart.LowPrice,
                    Volume = chart.Volume,
                    Period = TimeSpan.FromMinutes(1),
                    Time = Time.UnixMillisecondTimeStampToDateTime(chart.ChartTime)
                });
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
                {
                    symbolOrderBook = new DefaultOrderBook(symbolLean);
                    _subscribedTickers[symbolLean] = symbolOrderBook;
                }
                else
                {
                    symbolOrderBook.BestBidAskUpdated -= OnBestBidAskUpdated;
                    symbolOrderBook.Clear();
                }

                if (symbol.BidPrice > 0)
                {
                    symbolOrderBook.UpdateBidRow(symbol.BidPrice, symbol.BidSize);
                }

                if (symbol.AskPrice > 0)
                {
                    symbolOrderBook.UpdateAskRow(symbol.AskPrice, symbol.AskSize);
                }

                symbolOrderBook.BestBidAskUpdated += OnBestBidAskUpdated;

                if (symbol.LastPrice > 0 && symbol.LastSize > 0)
                {
                    var tradeTime = (Time.UnixTimeStampToDateTime(symbol.TradeTime));
                    var exchange = ConvertIDExchangeToFullName(symbol.ExchangeID);
                    var trade = new Tick(tradeTime, symbolLean, "", exchange, symbol.LastSize, symbol.LastPrice);
                    _aggregator.Update(trade);
                }
            }
        }

        private void OnBestBidAskUpdated(object? sender, BestBidAskUpdatedEventArgs e)
        {
            _aggregator.Update(new Tick
            {
                AskPrice = e.BestAskPrice,
                BidPrice = e.BestBidPrice,
                Time = DateTime.UtcNow,
                Symbol = e.Symbol,
                TickType = TickType.Quote,
                AskSize = e.BestAskSize,
                BidSize = e.BestBidSize
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
                    Log.Trace($"TDAmeritradeBrokerage:DataQueueHandler:OnMessage:AccountAcctivity: subscribed successfully, Description: {accountActivityData.Value.MessageData}");
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

            if(cashedOrder == null)
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
    }
}
