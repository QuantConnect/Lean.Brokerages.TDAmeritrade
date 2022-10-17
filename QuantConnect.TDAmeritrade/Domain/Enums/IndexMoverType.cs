using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    public enum IndexMoverType
    {
        [EnumMember(Value = "$DJI")]
        DJI = 0,
        [EnumMember(Value = "$COMPX")]
        COMPX = 1,
        [EnumMember(Value = "$SPX.X")]
        SPX_X = 2,
    }
}
