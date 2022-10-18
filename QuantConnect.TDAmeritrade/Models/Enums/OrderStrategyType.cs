using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum OrderStrategyType
    {
        [EnumMember(Value = "SINGLE")]
        Single = 0,
        [EnumMember(Value = "OCO")]
        Oco = 1,
        [EnumMember(Value = "TRIGGER")]
        Trigger = 2,
    }
}
