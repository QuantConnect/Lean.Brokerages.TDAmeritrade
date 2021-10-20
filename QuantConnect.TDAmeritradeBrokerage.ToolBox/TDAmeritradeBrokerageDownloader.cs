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
 *
*/

using QuantConnect.Brokerages.TDAmeritrade;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using TDAmeritradeApi.Client;

namespace QuantConnect.TDAmeritradeDownloader.ToolBox
{
    public class TDAmeritradeBrokerageDownloader : IDataDownloader
    {
        public TDAmeritradeBrokerageDownloader()
        {
            var clientID = Config.Get("td-client-id", "");
            var redirectUri = Config.Get("td-redirect-uri", "");
            var tdCredentials = Composer.Instance.GetExportedValueByTypeName<ICredentials>(Config.Get("td-credentials-provider", "QuantConnect.Brokerages.TDAmeritrade.TDCliCredentialProvider"));
            TDAmeritradeBrokerage.InitializeClient(clientID, redirectUri, tdCredentials);
        }

        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc)
        {
            return TDAmeritradeBrokerage.GetPriceHistory(symbol, resolution, startUtc, endUtc, TimeZones.NewYork);
        }
    }
}
