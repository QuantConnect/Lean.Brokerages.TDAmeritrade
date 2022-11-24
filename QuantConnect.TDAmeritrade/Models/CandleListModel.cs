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
    /// <summary>
    /// Model for price history for a symbol
    /// </summary>
    public class CandleListModel
    {
        [JsonProperty(PropertyName = "candles")]
        public List<CandleModel> Candles { get; set; } = new();

        [JsonProperty(PropertyName = "empty")]
        public bool Empty { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;
    }

    public class CandleModel
    {
        /// Historical Data Bar: Close
        [JsonProperty(PropertyName = "close")]
        public decimal Close;

        /// Historical Data Bar: Date
        [JsonProperty(PropertyName = "datetime")]
        public decimal DateTime;

        /// Historical Data Bar: High
        [JsonProperty(PropertyName = "high")]
        public decimal High;

        /// Historical Data Bar: Low
        [JsonProperty(PropertyName = "low")]
        public decimal Low;

        /// Historical Data Bar: Open
        [JsonProperty(PropertyName = "open")]
        public decimal Open;

        /// Historical Data Bar: Volume
        [JsonProperty(PropertyName = "volume")]
        public long Volume;
    }
}
