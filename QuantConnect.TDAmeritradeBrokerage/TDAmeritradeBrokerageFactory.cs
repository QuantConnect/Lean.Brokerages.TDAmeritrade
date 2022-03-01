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
using System.IO;
using System.Collections.Generic;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using TDAmeritradeApi.Client;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    /// <summary>
    /// Provides an implementations of IBrokerageFactory that produces a TDAmeritradeBrokerage
    /// </summary>
    public class TDAmeritradeBrokerageFactory : BrokerageFactory
    {
        private readonly List<TDAmeritradeBrokerage> instances = new List<TDAmeritradeBrokerage>();

        /// <summary>
        /// Gets TD Ameritrade values from configuration
        /// </summary>
        public static class Configuration
        {
            internal static readonly string AccountIDConfigFileKey = "tda-account-id";
            internal static readonly string ConsumerKeyConfigFileKey = "tda-consumer-key";
            internal static readonly string CallbackUrlConfigFileKey = "tda-callback-url";
            internal static readonly string IsPaperTradingConfigFileKey = "tda-paper-trading";
            internal static readonly string CredentialsProviderConfigFileKey = "tda-credentials-provider";
            internal static readonly string SavedTokenDirectoryConfigFileKey = "tda-saved-token-directory";

            /// <summary>
            /// Gets the account ID to be used when instantiating a brokerage
            /// </summary>
            public static string AccountID => Config.Get(AccountIDConfigFileKey);

            /// <summary>
            /// Gets the access token from configuration
            /// </summary>
            public static string ConsumerKey => Config.Get(ConsumerKeyConfigFileKey);

            public static string CallbackUrl => Config.Get(CallbackUrlConfigFileKey);

            public static bool IsPaperTrading => Config.GetBool(IsPaperTradingConfigFileKey);

            public static string SavedTokenDirectory => Config.Get(SavedTokenDirectoryConfigFileKey, Directory.GetCurrentDirectory());

            public static ICredentials Credentials => Composer.Instance.GetExportedValueByTypeName<ICredentials>(Config.Get(CredentialsProviderConfigFileKey, typeof(TDCliCredentialProvider).FullName));
        }

        /// <summary>
        /// Initializes a new instance of he TDAmeritradeBrokerageFactory class
        /// </summary>
        public TDAmeritradeBrokerageFactory()
            : base(typeof(TDAmeritradeBrokerage))
        {
        }

        /// <summary>
        /// Gets the brokerage data required to run the brokerage from configuration/disk
        /// </summary>
        /// <remarks>
        /// The implementation of this property will create the brokerage data dictionary required for
        /// running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
        /// </remarks>
        public override Dictionary<string, string> BrokerageData
        {
            get
            {
                var data = new Dictionary<string, string>
                {
                    { Configuration.AccountIDConfigFileKey, Configuration.AccountID.ToStringInvariant() },
                    { Configuration.ConsumerKeyConfigFileKey, Configuration.ConsumerKey.ToStringInvariant() },
                    { Configuration.CallbackUrlConfigFileKey, Configuration.CallbackUrl.ToStringInvariant() },
                    { Configuration.IsPaperTradingConfigFileKey, Configuration.IsPaperTrading.ToStringInvariant() },
                    { Configuration.SavedTokenDirectoryConfigFileKey, Configuration.SavedTokenDirectory.ToStringInvariant() },
                };
                return data;
            }
        }

        /// <summary>
        /// Gets a new instance of the <see cref="TDAmeritradeBrokerageModel"/>
        /// </summary>
        /// <param name="orderProvider">The order provider</param>
        public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new TDAmeritradeBrokerageModel();

        /// <summary>
        /// Creates a new IBrokerage instance
        /// </summary>
        /// <param name="job">The job packet to create the brokerage for</param>
        /// <param name="algorithm">The algorithm instance</param>
        /// <returns>A new brokerage instance</returns>
        public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
        {
            var errors = new List<string>();
            var accountId = Read<string>(job.BrokerageData, Configuration.AccountIDConfigFileKey, errors);
            var clientId = Read<string>(job.BrokerageData, Configuration.ConsumerKeyConfigFileKey, errors);
            var redirectUri = Read<string>(job.BrokerageData, Configuration.CallbackUrlConfigFileKey, errors);
            bool isPaperTrading = Read<bool>(job.BrokerageData, Configuration.IsPaperTradingConfigFileKey, errors);
            var savedTokenDirectory = Read<string>(job.BrokerageData, Configuration.SavedTokenDirectoryConfigFileKey, errors);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var tdBrokerage = new TDAmeritradeBrokerage(
                algorithm,
                algorithm.Transactions,
                algorithm.Portfolio,
                accountId,
                clientId,
                redirectUri,
                isPaperTrading,
                savedTokenDirectory
                );
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Add the brokerage to the composer to ensure its accessible to the live data feed.
            Composer.Instance.AddPart<IDataQueueUniverseProvider>(tdBrokerage);
            Composer.Instance.AddPart<IDataQueueHandler>(tdBrokerage);
            Composer.Instance.AddPart<IHistoryProvider>(tdBrokerage);
            Composer.Instance.AddPart<IOptionChainProvider>(tdBrokerage);

            instances.Add(tdBrokerage);

            return tdBrokerage;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            foreach (var instance in instances)
            {
                instance.DisposeSafely();
            }

            GC.SuppressFinalize(this);
        }
    }
}
