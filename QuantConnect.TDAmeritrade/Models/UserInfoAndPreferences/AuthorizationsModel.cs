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

using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct AuthorizationsModel
    {
        [JsonProperty(PropertyName = "apex")]
        public bool Apex { get; set; }

        [JsonProperty(PropertyName = "levelTwoQuotes")]
        public bool LevelTwoQuotes { get; set; }

        [JsonProperty(PropertyName = "stockTrading")]
        public bool StockTrading { get; set; }

        [JsonProperty(PropertyName = "marginTrading")]
        public bool MarginTrading { get; set; }

        [JsonProperty(PropertyName = "streamingNews")]
        public bool StreamingNews { get; set; }

        /// <summary>
        /// 'COVERED' or 'FULL' or 'LONG' or 'SPREAD' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "optionTradingLevel")]
        public string OptionTradingLevel { get; set; }

        [JsonProperty(PropertyName = "streamerAccess")]
        public bool StreamerAccess { get; set; }

        [JsonProperty(PropertyName = "advancedMargin")]
        public bool AdvancedMargin { get; set; }

        [JsonProperty(PropertyName = "scottradeAccount")]
        public bool ScottradeAccount { get; set; }

        [JsonProperty(PropertyName = "autoPositionEffect")]
        public bool AutoPositionEffect { get; set; }
    }
}
