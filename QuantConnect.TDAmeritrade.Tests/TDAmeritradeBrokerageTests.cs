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

using NodaTime;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Data.Market;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Brokerages.TDAmeritrade;

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

            return new TDAmeritradeBrokerage(_consumerKey, _codeFromUrl, _accountNumber, null, new AggregationManager(), orderProvider, TestGlobals.MapFileProvider);
        }

        protected override bool IsAsync() => true;
        protected override bool IsCancelAsync() => true;

        protected override decimal GetAskPrice(Symbol symbol)
        {
            var tradier = (TDAmeritradeBrokerage)Brokerage;
            var quotes = tradier.GetQuotes(symbol);
            return quotes.Single().AskPrice;
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void GetQuotesDoesNotReturnNull()
        {
            var tradier = (TDAmeritradeBrokerage)Brokerage;
            var quotes = tradier.GetQuotes(Symbol.Value);

            Assert.IsNotNull(quotes);
            Assert.IsNotEmpty(quotes);
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void PlaceOrderMarket()
        {
            var symbol = Symbols.LODE;

            var order = new MarketOrder(symbol, 1, DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void PlaceOrderLimit()
        {
            var symbol = Symbols.LODE;

            var price = ((TDAmeritradeBrokerage)Brokerage).GetQuote(symbol.Value).LastPrice;

            var order = new LimitOrder(symbol, 1, price + (price * 0.1m), DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void PlaceOrderStopLimit()
        {
            var symbol = Symbols.LODE;

            var price = ((TDAmeritradeBrokerage)Brokerage).GetQuote(symbol.Value).LastPrice;

            var order = new StopLimitOrder(symbol, 1, price + (price * 0.1m), price + (price * 0.2m), DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }
    }
}
