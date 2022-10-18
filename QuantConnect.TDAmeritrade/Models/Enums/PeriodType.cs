using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum PeriodType
    {
        [EnumMember(Value = "day")]
        Day = 0,
        [EnumMember(Value = "month")]
        Month = 1,
        [EnumMember(Value = "year")]
        Year = 2,
        /// <summary>
        /// Year to date
        /// </summary>
        [EnumMember(Value = "ytd")]
        Ytd = 3,
    }
}
