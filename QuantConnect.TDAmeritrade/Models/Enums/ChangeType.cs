using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum ChangeType
    {
        [EnumMember(Value = "No Value")]
        NoValue = 0,
        [EnumMember(Value = "value")]
        Value = 1,
        [EnumMember(Value = "percent")]
        Percent = 2,
    }
}
