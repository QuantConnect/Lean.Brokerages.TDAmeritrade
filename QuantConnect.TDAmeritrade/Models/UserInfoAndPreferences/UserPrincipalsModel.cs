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

        [JsonProperty(PropertyName = "quotes")]
        public QuotesModel Quotes { get; set; }

        [JsonProperty(PropertyName = "exchangeAgreements")]
        public ExchangeAgreementsModel ExchangeAgreements { get; set; }

        [JsonProperty(PropertyName = "accounts")]
        public List<AccountsModel> Accounts { get; set; } = new();
    }
}