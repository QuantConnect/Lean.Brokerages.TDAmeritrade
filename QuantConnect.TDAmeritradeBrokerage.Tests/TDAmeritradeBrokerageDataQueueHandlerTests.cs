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
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Tests;
using QuantConnect.Logging;
using QuantConnect.Data.Market;
using QuantConnect.Brokerages.TDAmeritrade;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace QuantConnect.TDAmeritradeDownloader.Tests
{
    [TestFixture]
    public partial class TDAmeritradeBrokerageTests
    {
        private static TestCaseData[] TestParameters
        {
            get
            {
                return new[]
                {
                    // valid parameters, for example
                    new TestCaseData(Symbols.SPY, Resolution.Tick, false),
                    new TestCaseData(Symbols.SPY, Resolution.Minute, false),
                    new TestCaseData(Symbols.SPY, Resolution.Second, false),
                };
            }
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void StreamsData(Symbol symbol, Resolution resolution, bool throwsException)
        {
            var cancelationToken = new CancellationTokenSource();
            var brokerage = (TDAmeritradeBrokerage)Brokerage;

            var subscriptionManager = GetPrivateField<DataQueueHandlerSubscriptionManager>(brokerage, "_subscriptionManager");

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

            ConcurrentDictionary<MarketDataType, ConcurrentQueue<BaseData>> data = new ConcurrentDictionary<MarketDataType, ConcurrentQueue<BaseData>>();
            List<Task> tasks = new List<Task>();
            foreach (var config in configs)
            {
                tasks.Add(ProcessFeed(brokerage.Subscribe(config, (s, e) => { }),
                    cancelationToken,
                    (baseData) =>
                    {
                        if (baseData != null)
                        {
                            if (!data.ContainsKey(baseData.DataType))
                            {
                                data.TryAdd(baseData.DataType, new ConcurrentQueue<BaseData>());
                            }

                            data[baseData.DataType].Enqueue(baseData);
                        }
                    }));
            }

            var types = configs.Select(c => ConvertTo(c.TickType)).Distinct();

            WaitUntilCondition(() => data.Keys.All(t => types.Contains(t)), 20000);

            foreach (var config in configs)
            {
                brokerage.Unsubscribe(config);
            }

            WaitUntilCondition(() => !subscriptionManager.IsSubscribed(symbol, TickType.Trade) &&
                                    !subscriptionManager.IsSubscribed(symbol, TickType.Quote), 20000);

            cancelationToken.Cancel();

            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(60));
        }

        public static T GetPrivateField<T>(object @object, string name)
        {
            var field = @object
                .GetType()
                .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            return (T)field?.GetValue(@object);
        }

        private static void WaitUntilCondition(Func<bool> condition, long maxWaitTimeInMilliseconds)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!condition() && stopwatch.ElapsedMilliseconds < maxWaitTimeInMilliseconds)
            {
                Thread.Sleep(1);
            }
        }

        private MarketDataType ConvertTo(TickType tickType)
        {
            switch (tickType)
            {
                case TickType.Trade:
                    return MarketDataType.TradeBar;
                case TickType.Quote:
                    return MarketDataType.QuoteBar;
                //case TickType.OpenInterest: //Options and futures
                //    break;
                default:
                    return MarketDataType.Tick;
            }
        }
    }
}