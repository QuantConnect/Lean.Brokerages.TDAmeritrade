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


using QuantConnect.Data.Market;
using QuantConnect.Logging;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeTests
    {
        [Test]
        public void StreamsData()
        {
            var cancelationToken = new CancellationTokenSource();
            var brokerage = (TDAmeritradeBrokerage)Brokerage;

            var symbol = Symbols.AAPL;
            var symbol2 = Symbols.IBM;

            var configs = new[] {
                GetSubscriptionDataConfig<QuoteBar>(symbol, Resolution.Tick),
                GetSubscriptionDataConfig<TradeBar>(symbol2, Resolution.Tick)
                };

            foreach (var config in configs)
            {
                ProcessFeed(brokerage.Subscribe(config, (s, e) => { }),
                    cancelationToken,
                    (baseData) =>
                    {
                        if (baseData != null) { Log.Trace($"{baseData}"); }
                    });
            }

            Thread.Sleep(10000);

            foreach (var config in configs)
                brokerage.Unsubscribe(config);

            Thread.Sleep(5000);

            cancelationToken.Cancel();
        }
    }
}
