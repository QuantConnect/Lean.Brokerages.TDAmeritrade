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
    public struct AccountsModel
    {
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "accountCdDomainId")]
        public string AccountCdDomainId { get; set; }

        [JsonProperty(PropertyName = "company")]
        public string Company { get; set; }

        [JsonProperty(PropertyName = "segment")]
        public string Segment { get; set; }

        [JsonProperty(PropertyName = "surrogateIds")]
        public SurrogateIdsModel SurrogateIds { get; set; }

        [JsonProperty(PropertyName = "preferences")]
        public PreferencesModel Preferences { get; set; }

        [JsonProperty(PropertyName = "acl")]
        public string Acl { get; set; }

        [JsonProperty(PropertyName = "authorizations")]
        public AuthorizationsModel Authorizations { get; set; }
    }
}
