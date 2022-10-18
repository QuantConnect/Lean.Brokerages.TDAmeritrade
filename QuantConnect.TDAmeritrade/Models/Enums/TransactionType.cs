using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum TransactionType
    {
        [EnumMember(Value = "No Value")]
        NO_VALUE = 0,
        [EnumMember(Value = "ALL")]
        ALL = 1,
        [EnumMember(Value = "TRADE")]
        TRADE = 2,
        [EnumMember(Value = "BUY_ONLY")]
        BUY_ONLY = 3,
        [EnumMember(Value = "SELL_ONLY")]
        SELL_ONLY = 4,
        [EnumMember(Value = "CASH_IN_OR_CASH_OUT")]
        CASH_IN_OR_CASH_OUT = 5,
        [EnumMember(Value = "CHECKING")]
        CHECKING = 6,
        [EnumMember(Value = "DIVIDEND")]
        DIVIDEND = 7,
        [EnumMember(Value = "INTEREST")]
        INTEREST = 8,
        [EnumMember(Value = "OTHER")]
        OTHER = 9,
        [EnumMember(Value = "ADVISOR_FEES")]
        ADVISOR_FEES = 10,
    }
}
