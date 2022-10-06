using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    /// <summary>
    /// The grant type of the oAuth scheme.
    /// </summary>
    public enum GrantType
    {
        [EnumMember(Value = "authorization_code")]
        AuthorizationCode = 0,
        [EnumMember(Value = "refresh_token")]
        RefreshToken = 1
    }
}
