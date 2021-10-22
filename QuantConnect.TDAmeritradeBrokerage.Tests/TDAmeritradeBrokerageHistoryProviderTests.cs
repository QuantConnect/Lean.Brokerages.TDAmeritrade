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

using System;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Tests;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Data.Market;
using QuantConnect.Lean.Engine.HistoricalData;
using QuantConnect.Brokerages.TDAmeritrade;
using System.Collections.Generic;
using QuantConnect.Configuration;
using QuantConnect.Util;
using TDAmeritradeApi.Client;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.TDAmeritradeDownloader.Tests
{
    [TestFixture]
    public class TDAmeritradeBrokerageHistoryProviderTests
    {
        private static TestCaseData[] TestParameters
        {
            get
            {
                return new[]
                {
                    // valid parameters, example:
                    //does not support tick data or quote data just trade bars
                    new TestCaseData(Symbols.SPY, Resolution.Tick, TimeSpan.FromMinutes(1), TickType.Quote, typeof(Tick), false),
                    new TestCaseData(Symbols.SPY, Resolution.Minute, TimeSpan.FromMinutes(10), TickType.Quote, typeof(QuoteBar), false),
                    new TestCaseData(Symbols.SPY, Resolution.Hour, TimeSpan.FromHours(10), TickType.Quote, typeof(QuoteBar), false),
                    new TestCaseData(Symbols.SPY, Resolution.Daily, TimeSpan.FromDays(10), TickType.Quote, typeof(QuoteBar), false),

                    new TestCaseData(Symbols.SPY, Resolution.Tick, TimeSpan.FromMinutes(1), TickType.Trade, typeof(Tick), false),
                    new TestCaseData(Symbols.SPY, Resolution.Minute, TimeSpan.FromMinutes(10), TickType.Trade, typeof(TradeBar), false),
                    new TestCaseData(Symbols.SPY, Resolution.Hour, TimeSpan.FromHours(10), TickType.Trade, typeof(TradeBar), false),
                    new TestCaseData(Symbols.SPY, Resolution.Daily, TimeSpan.FromDays(10), TickType.Trade, typeof(TradeBar), false),
                };
            }
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void GetsHistory(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType, Type dataType, bool throwsException)
        {
            TestDelegate test = () =>
            {
                var accountId = TDAmeritradeBrokerageFactory.Configuration.AccountID;

                var brokerage = new TDAmeritradeBrokerage(null, null, null, accountId);

                var historyProvider = new BrokerageHistoryProvider();
                historyProvider.SetBrokerage(brokerage);
                historyProvider.Initialize(new HistoryProviderInitializeParameters(null, null, null,
                    null, null, null, null,
                    false, new DataPermissionManager()));

                var marketHoursDatabase = MarketHoursDatabase.FromDataFolder();
                var now = DateTime.UtcNow;
                var requests = new[]
                {
                    new HistoryRequest(now.Add(-period),
                        now,
                        dataType,
                        symbol,
                        resolution,
                        marketHoursDatabase.GetExchangeHours(symbol.ID.Market, symbol, symbol.SecurityType),
                        marketHoursDatabase.GetDataTimeZone(symbol.ID.Market, symbol, symbol.SecurityType),
                        resolution,
                        false,
                        false,
                        DataNormalizationMode.Adjusted,
                        tickType)
                };

                bool foundTick = false, foundQuote = false, foundTradeBar = false;
                foreach (var slice in historyProvider.GetHistory(requests, TimeZones.Utc))
                {
                    if (resolution == Resolution.Tick)
                    {
                        foreach (var tick in slice.Ticks[symbol])
                        {
                            Log.Trace($"{tick}");
                            foundTick = true;
                        }
                    }
                    else if(slice.QuoteBars.TryGetValue(symbol, out var quoteBar))
                    {
                        Log.Trace($"{quoteBar}");
                        foundQuote = true;
                    }
                    else if(slice.Bars.TryGetValue(symbol, out var tradeBar))
                    {
                        Log.Trace($"{tradeBar}");
                        foundTradeBar = true;
                    }
                }

                if (tickType == TickType.Quote || dataType == typeof(Tick))
                {
                    Assert.IsTrue(historyProvider.DataPointCount == 0);
                }
                else
                {
                    Log.Trace("Data points retrieved: " + historyProvider.DataPointCount);
                    Assert.AreEqual(foundTick, dataType == typeof(Tick));
                    Assert.AreEqual(foundQuote, dataType == typeof(QuoteBar));
                    Assert.AreEqual(foundTradeBar, dataType == typeof(TradeBar));
                    Assert.IsTrue(historyProvider.DataPointCount > 0);
                }
            };

            if (throwsException)
            {
                Assert.Throws<ArgumentException>(test);
            }
            else
            {
                Assert.DoesNotThrow(test);
            }
        }
    }
}