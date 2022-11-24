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
    public struct SurrogateIdsModel
    {
        [JsonProperty(PropertyName = "SCARR")]
        public string SCARR { get; set; }

        [JsonProperty(PropertyName = "Market Edge")]
        public string MarketEdge { get; set; }

        [JsonProperty(PropertyName = "Zacks")]
        public string Zacks { get; set; }

        [JsonProperty(PropertyName = "Localytics")]
        public string Localytics { get; set; }

        [JsonProperty(PropertyName = "Market Watch")]
        public string MarketWatch { get; set; }

        [JsonProperty(PropertyName = "Flybits")]
        public string Flybits { get; set; }

        [JsonProperty(PropertyName = "BOZEL")]
        public string BOZEL { get; set; }

        [JsonProperty(PropertyName = "WallStreetStrategies")]
        public string WallStreetStrategies { get; set; }

        [JsonProperty(PropertyName = "STS")]
        public string STS { get; set; }

        [JsonProperty(PropertyName = "SiteCatalyst")]
        public string SiteCatalyst { get; set; }

        [JsonProperty(PropertyName = "OpinionLab")]
        public string OpinionLab { get; set; }

        [JsonProperty(PropertyName = "BriefingTrader")]
        public string BriefingTrader { get; set; }

        [JsonProperty(PropertyName = "WSOD")]
        public string WSOD { get; set; }

        [JsonProperty(PropertyName = "SP")]
        public string SP { get; set; }

        [JsonProperty(PropertyName = "DART")]
        public string DART { get; set; }

        [JsonProperty(PropertyName = "EF")]
        public string EF { get; set; }

        [JsonProperty(PropertyName = "GK")]
        public string GK { get; set; }

        [JsonProperty(PropertyName = "ePay")]
        public string EPay { get; set; }

        [JsonProperty(PropertyName = "VB")]
        public string VB { get; set; }

        [JsonProperty(PropertyName = "Layer")]
        public string Layer { get; set; }

        [JsonProperty(PropertyName = "PWS")]
        public string PWS { get; set; }

        [JsonProperty(PropertyName = "Investools")]
        public string Investools { get; set; }

        [JsonProperty(PropertyName = "MIN")]
        public string MIN { get; set; }

        [JsonProperty(PropertyName = "MGP")]
        public string MGP { get; set; }

        [JsonProperty(PropertyName = "VCE")]
        public string VCE { get; set; }

        [JsonProperty(PropertyName = "HAVAS")]
        public string HAVAS { get; set; }

        [JsonProperty(PropertyName = "MSTAR")]
        public string MSTAR { get; set; }
    }
}
