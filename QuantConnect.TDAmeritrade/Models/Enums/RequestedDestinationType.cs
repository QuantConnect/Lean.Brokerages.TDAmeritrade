using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum RequestedDestinationType
    {
        [EnumMember(Value = "INET")]
        Inet = 0,
        [EnumMember(Value = "ECN_ARCA")]
        EcnArca = 1,
        [EnumMember(Value = "CBOE")]
        Cboe = 2,
        [EnumMember(Value = "AMEX")]
        Amex = 3,
        [EnumMember(Value = "PHLX")]
        Phlx = 4,
        [EnumMember(Value = "ISE")]
        Ise = 5,
        [EnumMember(Value = "BOX")]
        Box = 6,
        [EnumMember(Value = "NYSE")]
        Nyse = 7,
        [EnumMember(Value = "NASDAQ")]
        Nasdaq = 8,
        [EnumMember(Value = "BATS")]
        Bats = 9,
        [EnumMember(Value = "C2")]
        C2 = 10,
        [EnumMember(Value = "AUTO")]
        AUTO = 11
    }
}
