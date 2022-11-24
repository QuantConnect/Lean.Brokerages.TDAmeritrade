﻿/*
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
    [JsonObject(Title = "orderLegCollection")]
    public class OrderLegCollectionModel : PlaceOrderLegCollectionModel
    {
        [JsonProperty(PropertyName = "orderLegType")]
        public string OrderLegType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "legId")]
        public ulong LegId { get; set; }

        [JsonProperty(PropertyName = "positionEffect")]
        public string PositionEffect { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "instrument")]
        public new InstrumentModel Instrument { get; set; } = new();
    }
}
