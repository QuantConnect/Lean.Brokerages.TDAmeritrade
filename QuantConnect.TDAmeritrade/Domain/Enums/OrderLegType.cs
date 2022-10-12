using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    public enum OrderLegType
    {
        [EnumMember(Value = "EQUITY")]
        Equity = 0,
        [EnumMember(Value = "OPTION")]
        Option = 1,
        [EnumMember(Value = "INDEX")]
        Index = 2,
        [EnumMember(Value = "MUTUAL_FUND")]
        MutualFund = 3,
        [EnumMember(Value = "CASH_EQUIVALENT")]
        CashEquivalent = 4,
        [EnumMember(Value = "FIXED_INCOME")]
        FixedIncome = 5,
        [EnumMember(Value = "CURRENCY")]
        Currency = 2,
    }
}
