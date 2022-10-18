using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    /// <summary>
    /// The market for which you're requesting market hours. 
    /// Valid markets are EQUITY, OPTION, FUTURE, BOND, or FOREX.
    /// </summary>
    public enum MarketType
    {
        [EnumMember(Value = "EQUITY")]
        EQUITY = 0,
        [EnumMember(Value = "OPTION")]
        OPTION = 1,
        [EnumMember(Value = "FUTURE")]
        FUTURE = 2,
        [EnumMember(Value = "BOND")]
        BOND = 3,
        [EnumMember(Value = "FOREX")]
        FOREX = 4,
    }
}
