namespace QuantConnect.Tests.Brokerages.TDAmeritrade
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
