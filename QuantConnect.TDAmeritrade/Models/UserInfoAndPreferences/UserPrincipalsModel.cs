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
    public class UserPrincipalsModel
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "userCdDomainId")]
        public string UserCdDomainId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "primaryAccountId")]
        public string PrimaryAccountId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "lastLoginTime")]
        public string LastLoginTime { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "tokenExpirationTime")]
        public string TokenExpirationTime { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "loginTime")]
        public string LoginTime { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "accessLevel")]
        public string AccessLevel { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "stalePassword")]
        public bool StalePassword { get; set; }

        [JsonProperty(PropertyName = "streamerInfo")]
        public StreamerInfoModel StreamerInfo { get; set; }

        /// <summary>
        /// 'PROFESSIONAL' or 'NON_PROFESSIONAL' or 'UNKNOWN_STATUS'
        /// </summary>
        [JsonProperty(PropertyName = "professionalStatus")]
        public string ProfessionalStatus { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "streamerSubscriptionKeys")]
        public StreamerSubscriptionKeys StreamerSubscriptionKeys { get; set; }

        [JsonProperty(PropertyName = "quotes")]
        public QuotesModel Quotes { get; set; }

        [JsonProperty(PropertyName = "exchangeAgreements")]
        public ExchangeAgreementsModel ExchangeAgreements { get; set; }

        [JsonProperty(PropertyName = "accounts")]
        public List<AccountsModel> Accounts { get; set; } = new();
    }
}