using QuantConnect.Configuration;

namespace QuantConnect.TDAmeritrade.Tests
{
    public class TDAmeritradeTests
    {
        private Application.TDAmeritrade _brokerage;

        private readonly string _consumerKey = Config.Get("tdameritrade-consumer-key");

        [OneTimeSetUp]
        public void Setup() => _brokerage = new Application.TDAmeritrade(_consumerKey);

        [TestCase("037833100")] // Apple Inc. [AAPL]
        public void GetInstrumentByCUSIP(string cusip)
        {
            var instrument = _brokerage.GetInstrumentByCUSIP(cusip);

            Assert.IsNotNull(instrument);
            Assert.IsNotEmpty(instrument.Cusip);
            Assert.IsNotEmpty(instrument.Symbol);
            Assert.IsNotEmpty(instrument.Description);
            Assert.IsNotEmpty(instrument.Exchange);
            Assert.IsNotEmpty(instrument.AssetType);
        }

        [TestCase("AAPL")]
        public void GetSearchInstrument(string symbol)
        {
            var instrument = _brokerage.GetSearchInstruments(symbol);

            Assert.IsNotNull(instrument);
        }
    }
}
