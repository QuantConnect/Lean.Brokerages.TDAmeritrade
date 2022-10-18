using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum FrequencyType
    {
        [EnumMember(Value = "novalue")]
        NoValue = 0,
        [EnumMember(Value = "minute")]
        Minute = 1,
        [EnumMember(Value = "daily")]
        Daily = 2,
        [EnumMember(Value = "weekly")]
        Weekly = 3,
        [EnumMember(Value = "monthly")]
        Monthly = 4,
    }
}
