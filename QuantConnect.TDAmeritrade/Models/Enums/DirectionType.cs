using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum DirectionType
    {
        [EnumMember(Value = "No Value")]
        NoValue = 0,
        [EnumMember(Value = "up")]
        Up = 1,
        [EnumMember(Value = "down")]
        Down = 2,
    }
}
