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
using QuantConnect.Data.Market;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeTests
    {
        [Explicit("This test requires a configured and testable account")]
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

        private static IEnumerable<TestCaseData> InvalidHistoryTestCases
        {
            get
            {
                // invalid security type
                yield return new TestCaseData(Symbols.BTCUSD, Resolution.Daily, TickType.Trade, TimeSpan.FromDays(15));
                yield return new TestCaseData(Symbols.SPY_C_192_Feb19_2016, Resolution.Daily, TickType.Trade, TimeSpan.FromDays(15));

                // invalid resolution
                yield return new TestCaseData(Symbols.AAPL, Resolution.Tick, TickType.Trade, TimeSpan.FromDays(15));
                yield return new TestCaseData(Symbols.AAPL, Resolution.Second, TickType.Trade, TimeSpan.FromDays(15));

                // invalid tick type
                yield return new TestCaseData(Symbols.AAPL, Resolution.Daily, TickType.Quote, TimeSpan.FromDays(15));
                yield return new TestCaseData(Symbols.AAPL, Resolution.Daily, TickType.OpenInterest, TimeSpan.FromDays(15));

                // invalid date range
                yield return new TestCaseData(Symbols.AAPL, Resolution.Daily, TickType.Trade, TimeSpan.FromDays(0));
                yield return new TestCaseData(Symbols.AAPL, Resolution.Daily, TickType.Trade, TimeSpan.FromDays(-15));
            }
        }

        [Explicit("This test requires a configured and testable account")]
        [TestCaseSource(nameof(InvalidHistoryTestCases))]
        public void HistoryReturnsNullForUnsupportedRequests(Symbol symbol, Resolution resolution, TickType tickType, TimeSpan period)
        {
            var endTime = new DateTime(2022, 08, 19);
            var historyRequest = new HistoryRequest(
                new SubscriptionDataConfig(typeof(TradeBar), symbol, resolution, DateTimeZone.Utc, DateTimeZone.Utc, true, true, true, tickType: tickType),
                SecurityExchangeHours.AlwaysOpen(DateTimeZone.Utc),
                endTime.Subtract(period),
                endTime);

            var history = Brokerage.GetHistory(historyRequest);

            Assert.IsNull(history);
        }
    }
}
