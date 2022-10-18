using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum SessionType
    {
        [EnumMember(Value = "NORMAL")]
        Normal = 0,
        [EnumMember(Value = "AM")]
        AM = 1,
        [EnumMember(Value = "PM")]
        PM = 2,
        [EnumMember(Value = "SEAMLESS")]
        Seamless = 3
    }
}
