using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    public enum InstructionType
    {
        [EnumMember(Value = "BUY")]
        Buy = 0,
        [EnumMember(Value = "SELL")]
        Sell = 1,
        [EnumMember(Value = "BUY_TO_COVER")]
        BuyToCover = 2,
        [EnumMember(Value = "BUY_TO_OPEN")]
        BuyToOpen = 3,
        [EnumMember(Value = "BUY_TO_CLOSE")]
        BuyToClose = 4,
        [EnumMember(Value = "SELL_TO_OPEN")]
        SellToOpen = 5,
        [EnumMember(Value = "SELL_TO_CLOSE")]
        SellToClose = 6,
        [EnumMember(Value = "EXCHANGE")]
        Exchange = 7,
    }
}
