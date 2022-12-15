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
using QuantConnect.Data;
using QuantConnect.Logging;
using QuantConnect.Data.Market;
using QuantConnect.Brokerages.TDAmeritrade;
using System.Collections.Concurrent;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    [TestFixture]
    public partial class TDAmeritradeTests
    {
        private static TestCaseData[] TestParameters
        {
            get
            {
                return new[]
                {
                    new TestCaseData(Symbols.AAPL, Resolution.Tick, false),
                    new TestCaseData(Symbols.AAPL, Resolution.Minute, false),
                    new TestCaseData(Symbols.AAPL, Resolution.Second, false)
                };
            }
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void StreamsData(Symbol symbol, Resolution resolution, bool throwsException)
        {
            var cancelationToken = new CancellationTokenSource();
            var brokerage = (TDAmeritradeBrokerage)Brokerage;

            SubscriptionDataConfig[] configs;
            if (resolution == Resolution.Tick)
            {
                var tradeConfig = new SubscriptionDataConfig(GetSubscriptionDataConfig<Tick>(symbol, resolution), tickType: TickType.Trade);
                var quoteConfig = new SubscriptionDataConfig(GetSubscriptionDataConfig<Tick>(symbol, resolution), tickType: TickType.Quote);
                configs = new[] { tradeConfig, quoteConfig };
            }
            else
            {
                configs = new[] { GetSubscriptionDataConfig<QuoteBar>(symbol, resolution),
                    GetSubscriptionDataConfig<TradeBar>(symbol, resolution) };
            }

            var dataReceivedForType = new Dictionary<Type, int>() { { typeof(Tick), 0}, { typeof(TradeBar), 0}, { typeof(QuoteBar), 0} };

            foreach (var config in configs)
            {
                ProcessFeed(brokerage.Subscribe(config, (s, e) => { }),
                    cancelationToken,
                    (tick) =>
                    {
                        if(tick is Tick)
                        {
                            Log.Debug($"Tick: {tick}");
                            Assert.NotZero(tick.Price);
                            Assert.IsTrue(tick.Price > 0, "Price was not greater then zero");
                            Assert.IsTrue(tick.Value > 0, "Value was not greater then zero");
                            Assert.That(tick.Symbol, Is.EqualTo(config.Symbol));

                            dataReceivedForType[typeof(Tick)] += 1;
                        }
                        if (tick is TradeBar tradeBar)
                        {
                            Log.Debug($"TradeBar: {tick}");
                            Assert.That(tick.Symbol, Is.EqualTo(config.Symbol));
                            Assert.IsTrue(tick.DataType == MarketDataType.TradeBar);
                            Assert.IsTrue(tick.Price > 0, "Price was not greater then zero");
                            Assert.IsTrue(tick.Value > 0, "Value was not greater then zero");

                            Assert.AreEqual(tradeBar.Period, config.Resolution.ToTimeSpan(), "Resolution was not equal period");

                            dataReceivedForType[typeof(TradeBar)] += 1;
                        }
                        if (tick is QuoteBar quoteBar)
                        {
                            Log.Debug($"QuoteBar: {tick}");
                            Assert.That(tick.Symbol, Is.EqualTo(config.Symbol));
                            Assert.IsTrue(tick.DataType == MarketDataType.QuoteBar);
                            Assert.IsTrue(tick.Price > 0, "Price was not greater then zero");
                            Assert.IsTrue(tick.Value > 0, "Value was not greater then zero");

                            Assert.AreEqual(quoteBar.Period, config.Resolution.ToTimeSpan(), "QuoteBar was not equal period");


                            dataReceivedForType[typeof(QuoteBar)] += 1;
                        }
                    });
            }

            Thread.Sleep(20000);

            foreach (var config in configs)
            {
                brokerage.Unsubscribe(config);
            }

            Thread.Sleep(10000);

            foreach (var dataType in dataReceivedForType.Keys)
            {
                if (configs.Any(i => i.Type == dataType))
                {
                    Assert.Greater(dataReceivedForType[dataType], 0);
                }
                else
                {
                    Assert.AreEqual(dataReceivedForType[dataType], 0);
                }
            }

            cancelationToken.Cancel();
        }

        [Test]
        public void MultipleSubscriptions()
        {
            var symbolSubscriptionResult = new Dictionary<Symbol, bool>();
            var symbolTickResult = new ConcurrentDictionary<Symbol, int>();
            var unsubscribeOneSymbolAngGetAnotherOne = new Dictionary<Symbol, bool>();
            var unsubscribeOneSymbol = true;

            var symbols = new List<(Symbol symbol, Resolution resolution)>
            {
                (Symbols.AAPL, Resolution.Minute),
                (Symbols.SPY, Resolution.Tick),
                (Symbols.LODE, Resolution.Minute),
                (Symbol.Create("BF-B", SecurityType.Equity, Market.USA), Resolution.Minute)
            };

            var configs = new List<SubscriptionDataConfig>();
            foreach (var symbol in symbols)
            {
                configs.Add(GetSubscriptionDataConfig<QuoteBar>(symbol.symbol, symbol.resolution));
            }

            var cancelationToken = new CancellationTokenSource();
            var brokerage = (TDAmeritradeBrokerage)Brokerage;

            foreach (var config in configs)
            {
                ProcessFeed(brokerage.Subscribe(config, (s, e) => { }),
                    cancelationToken,
                    (tick) =>
                    {
                        if (tick != null)
                        {
                            symbolTickResult.AddOrUpdate(tick.Symbol, 1, (Symbol, count) => count + 1);

                            symbolSubscriptionResult[tick.Symbol] = true;
                            unsubscribeOneSymbolAngGetAnotherOne[tick.Symbol] = true;

                            if (unsubscribeOneSymbol)
                            {
                                unsubscribeOneSymbol = false;
                                unsubscribeOneSymbolAngGetAnotherOne[tick.Symbol] = false;
                                brokerage.Unsubscribe(configs.First(x => x.Symbol == tick.Symbol));
                            }
                        }
                    });
            }

            Thread.Sleep(20000);

            foreach (var config in configs)
            {
                brokerage.Unsubscribe(config);
            }

            Thread.Sleep(10000);

            Assert.IsTrue(symbolSubscriptionResult.Values.All(value => value));
            Assert.IsFalse(unsubscribeOneSymbolAngGetAnotherOne.Values.All(value => value));
            Assert.IsTrue(symbolTickResult.Values.All(value => value > 0));

            cancelationToken.Cancel();
        }
    }
}
