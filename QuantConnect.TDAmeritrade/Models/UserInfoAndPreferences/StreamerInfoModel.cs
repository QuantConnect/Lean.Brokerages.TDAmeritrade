using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct StreamerInfoModel
    {
        [JsonProperty(PropertyName = "streamerBinaryUrl")]
        public string StreamerBinaryUrl { get; set; }

        [JsonProperty(PropertyName = "streamerSocketUrl")]
        public string StreamerSocketUrl { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "tokenTimestamp")]
        public string TokenTimestamp { get; set; }

        [JsonProperty(PropertyName = "userGroup")]
        public string UserGroup { get; set; }

        [JsonProperty(PropertyName = "accessLevel")]
        public string AccessLevel { get; set; }

        [JsonProperty(PropertyName = "acl")]
        public string Acl { get; set; }

        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }
    }
}
