using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    public enum DurationType
    {
        [EnumMember(Value = "DAY")]
        Day = 0,
        [EnumMember(Value = "GOOD_TILL_CANCEL")]
        GoodTillCancel = 1,
        [EnumMember(Value = "FILL_OR_KILL")]
        FullOrKill = 2,
    }
}
