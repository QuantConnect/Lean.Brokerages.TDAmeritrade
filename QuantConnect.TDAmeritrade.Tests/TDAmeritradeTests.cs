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


using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.Securities;
using NodaTime;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeTests : BrokerageTests
    {
        protected override Symbol Symbol => Symbol.Create("LODE", SecurityType.Equity, Market.USA);

        protected override SecurityType SecurityType => SecurityType.Equity;        

        protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            string _consumerKey = Config.Get("tdameritrade-consumer-key");
            string _callbackUrl = Config.Get("tdameritrade-callback-url");
            string _codeFromUrl = Config.Get("tdameritrade-code-from-url");
            string _refreshToken = Config.Get("tdameritrade-refresh-token");
            string _accountNumber = Config.Get("tdameritrade-account-number");

            return new TDAmeritradeBrokerage(_consumerKey, _refreshToken, _callbackUrl, _codeFromUrl, _accountNumber, null, securityProvider, new AggregationManager(), orderProvider);
        }

        protected override bool IsAsync() => true;
        protected override bool IsCancelAsync() => true;

        protected override decimal GetAskPrice(Symbol symbol)
        {
            var tradier = (TDAmeritradeBrokerage)Brokerage;
            var quotes = tradier.GetQuotes(symbol);
            return quotes.Single().AskPrice;
        }

        [Test]
        public void GetQuotesDoesNotReturnNull()
        {
            var tradier = (TDAmeritradeBrokerage)Brokerage;
            var quotes = tradier.GetQuotes(Symbol.Value);

            Assert.IsNotNull(quotes);
            Assert.IsNotEmpty(quotes);
        }

        [TestCase("AAPL", Resolution.Minute)]
        [TestCase("AAPL", Resolution.Hour)]
        [TestCase("AAPL", Resolution.Daily)]
        public void TestHistoryProvider_GetHistory(string ticker, Resolution resolution)
        {
            var symbol = Symbol.Create(ticker, SecurityType.Equity, Market.USA);

            DateTime startDateTime = DateTime.UtcNow.AddDays(-2.0);
            DateTime endDateTime = DateTime.UtcNow;

            var historyRequest = new HistoryRequest(
                new SubscriptionDataConfig(typeof(TradeBar), symbol, resolution, DateTimeZone.Utc, DateTimeZone.Utc, true, true, true),
                SecurityExchangeHours.AlwaysOpen(DateTimeZone.Utc),
                startDateTime,
                endDateTime);

            var histories = Brokerage.GetHistory(historyRequest);

            Assert.IsNotEmpty(histories);

            var history = histories.FirstOrDefault();

            Assert.IsNotNull(history);

            Assert.Greater(history.Price, 0m);
            Assert.Greater(history.Value, 0m);
            Assert.That(history.Symbol.Value, Is.EqualTo(ticker).NoClip);

            Assert.IsTrue(history.DataType == MarketDataType.TradeBar);

            TradeBar historyBar = (TradeBar)history;

            Assert.Greater(historyBar.Low, 0m);
            Assert.Greater(historyBar.Close, 0m);
            Assert.Greater(historyBar.High, 0m);
            Assert.Greater(historyBar.Open, 0m);

        }

        [Ignore("Ignore to save cash")]
        [Test]
        public void PlaceOrderMarket()
        {
            var symbol = Symbols.LODE;

            var order = new MarketOrder(symbol, 1, DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        [Ignore("Ignore to save cash")]
        [Test]
        public void PlaceOrderLimit()
        {
            var symbol = Symbols.LODE;

            var price = ((TDAmeritradeBrokerage)Brokerage).GetQuote(symbol.Value).LastPrice;

            var order = new LimitOrder(symbol, 1, price + (price * 0.1m), DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        //[Ignore("Ignore to save cash")]
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
