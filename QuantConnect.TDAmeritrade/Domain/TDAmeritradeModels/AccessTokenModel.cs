using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public class AccessTokenModel
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }
    }
}
