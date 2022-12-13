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
    public struct ChartEquityModel
    {
        [JsonProperty(PropertyName = "key")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "1")]
        public decimal OpenPrice { get; set; }

        [JsonProperty(PropertyName = "2")]
        public decimal HighPrice { get; set; }

        [JsonProperty(PropertyName = "3")]
        public decimal LowPrice { get; set; }

        [JsonProperty(PropertyName = "4")]
        public decimal ClosePrice { get; set; }

        [JsonProperty(PropertyName = "5")]
        public decimal Volume { get; set; }

        [JsonProperty(PropertyName = "6")]
        public decimal Sequence { get; set; }

        [JsonProperty(PropertyName = "7")]
        public decimal ChartTime { get; set; }

        [JsonProperty(PropertyName = "8")]
        public decimal ChartDay { get; set; }
    }
}
