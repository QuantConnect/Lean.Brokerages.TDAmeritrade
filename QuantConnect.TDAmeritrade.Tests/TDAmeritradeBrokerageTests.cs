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

using NUnit.Framework;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Configuration;
using QuantConnect.Brokerages.TDAmeritrade;
using QuantConnect.Brokerages;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeTests : BrokerageTests
    {
        protected override Symbol Symbol => Symbol.Create("LODE", SecurityType.Equity, Market.USA);

        protected override SecurityType SecurityType => SecurityType.Equity;

        protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            string _consumerKey = Config.Get("tdameritrade-api-key");
            string _codeFromUrl = Config.Get("tdameritrade-access-token");
            string _accountNumber = Config.Get("tdameritrade-account-number");

            return new TDAmeritradeBrokerage(_consumerKey, _codeFromUrl, _accountNumber, null, orderProvider);
        }

        protected override bool IsAsync() => true;
        protected override bool IsCancelAsync() => true;

        protected override decimal GetAskPrice(Symbol symbol)
        {
            var tdameritrade = (TDAmeritradeBrokerage)Brokerage;
            var quotes = tdameritrade.GetQuotes(symbol.Value);
            return quotes.Single().AskPrice;
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void GetQuotesDoesNotReturnNull()
        {
            var tdameritrade = (TDAmeritradeBrokerage)Brokerage;
            var quotes = tdameritrade.GetQuotes(Symbol.Value);

            Assert.IsNotNull(quotes);
            Assert.IsNotEmpty(quotes);
        }

        /// <summary>
        /// Provides the data required to test each order type in various cases
        /// </summary>
        private static TestCaseData[] OrderParameters()
        {
            return new[]
            {
                new TestCaseData(new MarketOrderTestParameters(Symbols.LODE)).SetName("MarketOrder"),
                new TestCaseData(new LimitOrderTestParameters(Symbols.LODE, 1m, 0.01m)).SetName("LimitOrder"),
                new TestCaseData(new StopMarketOrderTestParameters(Symbols.LODE, 1m, 0.01m)).SetName("StopMarketOrder"),
                new TestCaseData(new StopLimitOrderTestParameters(Symbols.LODE, 0.5m, 0.51m)).SetName("StopLimitOrder")
            };
        }

        [Explicit("This test requires a configured and testable account")]
        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void LongFromZero(OrderTestParameters parameters)
        {
            base.LongFromZero(parameters);
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void PlaceOrderThenUpdateOrder()
        {
            var brokerIdAfterUpdateOrder = string.Empty;
            List<Order> _orders = new List<Order>();
            var tdameritrade = (TDAmeritradeBrokerage)Brokerage;

            var limitOrder = new LimitOrder(Symbol, GetDefaultQuantity(), 0.24m, DateTime.UtcNow);

            EventHandler<List<OrderEvent>> brokerageOnOrderStatusChanged = (sender, args) =>
            {
                var orderEvent = args.Single();
                limitOrder.Status = orderEvent.Status;

                if (orderEvent.Status == OrderStatus.Canceled || orderEvent.Status == OrderStatus.Invalid)
                {
                    QuantConnect.Logging.Log.Trace("ModifyOrderUntilFilled(): " + limitOrder);
                    Assert.Fail("Unexpected order status: " + orderEvent.Status);
                }
            };

            EventHandler<BrokerageOrderIdChangedEvent> brokerageOrderIdChanged = (sender, args) => {
                brokerIdAfterUpdateOrder = args.BrokerId[0];
            };

            tdameritrade.OrderIdChanged += brokerageOrderIdChanged;
            tdameritrade.OrdersStatusChanged += brokerageOnOrderStatusChanged;

            OrderProvider.Add(limitOrder);            

            if (!tdameritrade.PlaceOrder(limitOrder))
            {
                Assert.Fail("Brokerage failed to place the order: " + limitOrder);
            }

            var brokerIdAfterPlaceOrder = OrderProvider.GetOpenOrders().Last().BrokerId[0];

            Assert.That(limitOrder.Status, Is.EqualTo(OrderStatus.Submitted));

            var request = new UpdateOrderRequest(DateTime.UtcNow, limitOrder.Id, new UpdateOrderFields { LimitPrice = limitOrder.LimitPrice / 2 });

            limitOrder.ApplyUpdateOrderRequest(request);

            if (!tdameritrade.UpdateOrder(limitOrder))
            {
                Assert.Fail("Brokerage failed to update the order: " + limitOrder);
            }

            Assert.That(brokerIdAfterUpdateOrder, Is.Not.EqualTo(brokerIdAfterPlaceOrder));

            Brokerage.OrderIdChanged -= brokerageOrderIdChanged;
            Brokerage.OrdersStatusChanged -= brokerageOnOrderStatusChanged;

        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void RejectedOrderForInvalidSymbol()
        {
            var message = string.Empty;
            EventHandler<BrokerageMessageEvent> messageHandler = (s, e) => { message = e.Message; };

            Brokerage.Message += messageHandler;

            var symbol = Symbol.Create("XYZ", SecurityType.Equity, Market.USA);
            var orderProperties = new OrderProperties();
            orderProperties.TimeInForce = TimeInForce.Day;
            PlaceOrderWaitForStatus(new MarketOrder(symbol, 1, DateTime.Now, properties: orderProperties), OrderStatus.Invalid, allowFailedSubmission: true);

            Brokerage.Message -= messageHandler;

            var messagee = Newtonsoft.Json.JsonConvert.DeserializeObject(message) as Newtonsoft.Json.Linq.JToken;

            Assert.That(messagee!["error"]!.ToString(), Is.EqualTo("Could not resolve instrument [AssetType: EQUITY, Symbol: XYZ]"));
        }
    }
}
