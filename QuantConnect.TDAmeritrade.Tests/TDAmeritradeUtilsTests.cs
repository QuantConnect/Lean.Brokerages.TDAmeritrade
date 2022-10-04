using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.TDAmeritrade.Utils.Extensions;

namespace QuantConnect.TDAmeritrade.Tests
{
    public class TDAmeritradeUtilsTests
    {
        [TestCase(PeriodType.Day)]
        [TestCase(PeriodType.Month)]
        [TestCase(PeriodType.Year)]
        public void ConvertPeriodTypeToStringFormat(PeriodType periodType)
        {
            string periodTypeStr = EnumConverterExtension.GetEnumMemberValue(periodType)!;

            Assert.IsNotEmpty(periodTypeStr);
        }
    }
}
