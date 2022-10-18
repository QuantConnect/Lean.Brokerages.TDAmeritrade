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
