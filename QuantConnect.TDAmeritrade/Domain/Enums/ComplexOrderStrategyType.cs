using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    public enum ComplexOrderStrategyType
    {
        [EnumMember(Value = "NONE")]
        None = 0,
        [EnumMember(Value = "COVERED")]
        Covered = 1,
        [EnumMember(Value = "VERTICAL")]
        Vertical = 2,
        [EnumMember(Value = "BACK_RATIO")]
        BackRatio = 3,
        [EnumMember(Value = "CALENDAR")]
        Calendar = 4,
        [EnumMember(Value = "DIAGONAL")]
        Diagonal = 5,
        [EnumMember(Value = "STRADDLE")]
        Straddle = 6,
        [EnumMember(Value = "STRANGLE")]
        Strangle = 7,
        [EnumMember(Value = "COLLAR_SYNTHETIC")]
        CollarSynthetic = 8,
        [EnumMember(Value = "BUTTERFLY")]
        Butterfly = 9,
        [EnumMember(Value = "CONDOR")]
        Condor = 10,
        [EnumMember(Value = "IRON_CONDOR")]
        IronCondor = 11,
        [EnumMember(Value = "VERTICAL_ROLL")]
        VerticalRoll = 12,
        [EnumMember(Value = "COLLAR_WITH_STOCK")]
        CollarWithStock = 13,
        [EnumMember(Value = "DOUBLE_DIAGONAL")]
        DoubleDiagonal = 14,
        [EnumMember(Value = "UNBALANCED_BUTTERFLY")]
        UnbalancedButterfly = 15,
        [EnumMember(Value = "UNBALANCED_CONDOR")]
        UnbalancedCondor= 16,
        [EnumMember(Value = "UNBALANCED_IRON_CONDOR")]
        UnbalancedIronCondor = 17,
        [EnumMember(Value = "UNBALANCED_VERTICAL_ROLL")]
        UnbalancedVerticalRoll = 18,
        [EnumMember(Value = "CUSTOM")]
        Custom = 19,
    }
}
