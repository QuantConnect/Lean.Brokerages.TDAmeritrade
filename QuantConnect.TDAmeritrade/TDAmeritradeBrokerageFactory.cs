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

using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    /// <summary>
    /// Factory type for the <see cref="TDAmeritradeBrokerage"/>
    /// </summary>
    public class TDAmeritradeBrokerageFactory : BrokerageFactory
    {
        public override Dictionary<string, string> BrokerageData
        {
            get
            {
                var data = new Dictionary<string, string>()
                {
                    { "tdameritrade-api-key", TDAmeritradeConfiguration.ConsumerKey.ToStringInvariant() },
                    { "tdameritrade-access-token", TDAmeritradeConfiguration.AccessToken.ToStringInvariant() },
                    { "tdameritrade-account-number", TDAmeritradeConfiguration.AccountNumber.ToStringInvariant() }
                };
                return data;
            }
        }

        public TDAmeritradeBrokerageFactory() : base(typeof(TDAmeritradeBrokerage))
        { }

        public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new TDAmeritradeBrokerageModel();

        public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
        {
            var errors = new List<string>();

            var consumerKey = Read<string>(job.BrokerageData, "tdameritrade-api-key", errors);
            var accessToken = Read<string>(job.BrokerageData, "tdameritrade-access-token", errors);
            var accountNumber = Read<string>(job.BrokerageData, "tdameritrade-account-number", errors);

            if (errors.Any())
            {
                throw new BrokerageException($"TDAmeritradeBrokerageFactory invalid config keys, errors:{String.Join(';', errors)}");
            }

            var brokerage = new TDAmeritradeBrokerage(consumerKey, accessToken, accountNumber, algorithm, algorithm.Transactions);

            // Add the brokerage to the composer to ensure its accessible to the live data feed.
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

        public override void Dispose()
        { }
    }
}
