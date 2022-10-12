using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
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
