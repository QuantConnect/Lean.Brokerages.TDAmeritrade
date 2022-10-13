using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels;

namespace QuantConnect.TDAmeritrade.Tests
{
    public class TDAmeritradeWebSocketTests
    {
        private Application.TDAmeritrade _brokerage;

        private readonly string _consumerKey = Config.Get("tdameritrade-consumer-key");
        private readonly string _callbackUrl = Config.Get("tdameritrade-callback-url");
        private readonly string _codeFromUrl = Config.Get("tdameritrade-code-from-url");
        private readonly string _refreshToken = Config.Get("tdameritrade-refresh-token");
        private readonly string _accountNumber = Config.Get("tdameritrade-account-number");

        [OneTimeSetUp]
        public void Setup() => _brokerage = new Application.TDAmeritrade(_consumerKey, _refreshToken, _callbackUrl, _codeFromUrl, _accountNumber, null);

        [Test]
        public void GetLoginRequstWS()
        {
            var row = _brokerage.Login();

            Assert.IsNotNull(row);
            Assert.IsNotEmpty(row);
        }
    }
}
