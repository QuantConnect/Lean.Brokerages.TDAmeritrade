using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.Securities;
using NodaTime;
using QuantConnect.Data.Market;

namespace QuantConnect.TDAmeritrade.Tests
{
    public class TDAmeritradeTests
    {
        private Application.TDAmeritrade _brokerage;

        private readonly string _consumerKey = Config.Get("tdameritrade-consumer-key");
        private readonly string _callbackUrl = Config.Get("tdameritrade-callback-url");
        private readonly string _codeFromUrl = Config.Get("tdameritrade-code-from-url");
        private readonly string _refreshToken = Config.Get("tdameritrade-refresh-token");

        [OneTimeSetUp]
        public void Setup() => _brokerage = new Application.TDAmeritrade(_consumerKey, _refreshToken, _callbackUrl, _codeFromUrl, null);

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

        [TestCase("AAPL", ProjectionType.SymbolSearch)]
        [TestCase("AAPL", ProjectionType.Fundamental)]
        public void GetSearchInstrument(string symbol, ProjectionType projectionType)
        {
            var instrument = _brokerage.GetSearchInstruments(symbol, projectionType);

            Assert.IsNotNull(instrument);
            Assert.IsNotEmpty(instrument.Cusip);
            Assert.IsNotEmpty(instrument.Symbol);
            Assert.IsNotEmpty(instrument.Description);
            Assert.IsNotEmpty(instrument.Exchange);
            Assert.IsNotEmpty(instrument.AssetType);

            if (instrument.Fundamental != null)
            {
                Assert.IsNotEmpty(instrument.Fundamental.Symbol);
                Assert.Greater(instrument.Fundamental.High52, 0);
                Assert.Greater(instrument.Fundamental.Low52, 0);
                Assert.Greater(instrument.Fundamental.DividendAmount, 0);
                Assert.Greater(instrument.Fundamental.DividendYield, 0);
                Assert.IsNotEmpty(instrument.Fundamental.DividendDate);
                Assert.Greater(instrument.Fundamental.PeRatio, 0);
                Assert.Greater(instrument.Fundamental.PegRatio, 0);
                Assert.Greater(instrument.Fundamental.PbRatio, 0);
                Assert.Greater(instrument.Fundamental.PrRatio, 0);
                Assert.Greater(instrument.Fundamental.PcfRatio, 0);
                Assert.Greater(instrument.Fundamental.GrossMarginTTM, 0);
                Assert.Greater(instrument.Fundamental.GrossMarginMRQ, 0);
                Assert.Greater(instrument.Fundamental.NetProfitMarginTTM, 0);
                Assert.Greater(instrument.Fundamental.NetProfitMarginMRQ, 0);
                Assert.Greater(instrument.Fundamental.OperatingMarginTTM, 0);
                Assert.Greater(instrument.Fundamental.OperatingMarginMRQ, 0);
                Assert.Greater(instrument.Fundamental.ReturnOnEquity, 0);
                Assert.Greater(instrument.Fundamental.ReturnOnAssets, 0);
                Assert.Greater(instrument.Fundamental.ReturnOnInvestment, 0);
                Assert.Greater(instrument.Fundamental.QuickRatio, 0);
                Assert.Greater(instrument.Fundamental.CurrentRatio, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.InterestCoverage, 0);
                Assert.Greater(instrument.Fundamental.TotalDebtToCapital, 0);
                Assert.Greater(instrument.Fundamental.LtDebtToEquity, 0);
                Assert.Greater(instrument.Fundamental.TotalDebtToEquity, 0);
                Assert.Greater(instrument.Fundamental.EpsTTM, 0);
                Assert.Greater(instrument.Fundamental.EpsChangePercentTTM, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.EpsChangeYear, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.EpsChange, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.RevChangeYear, 0);
                Assert.Greater(instrument.Fundamental.RevChangeTTM, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.RevChangeIn, 0);
                Assert.Greater(instrument.Fundamental.SharesOutstanding, 0);
                Assert.Greater(instrument.Fundamental.MarketCapFloat, 0);
                Assert.Greater(instrument.Fundamental.MarketCap, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.BookValuePerShare, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.ShortIntToFloat, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.ShortIntDayToCover, 0);
                Assert.GreaterOrEqual(instrument.Fundamental.DivGrowthRate3Year, 0);
                Assert.IsNotEmpty(instrument.Fundamental.DividendPayDate);
                Assert.Greater(instrument.Fundamental.Beta, 0);
                Assert.Greater(instrument.Fundamental.Vol1DayAvg, 0);
                Assert.Greater(instrument.Fundamental.Vol10DayAvg, 0);
                Assert.Greater(instrument.Fundamental.Vol3MonthAvg, 0);
            }
        }

        [TestCase("AAPL")]
        public void GetPriceHistory(string ticker)
        {
            var symbol = Symbol.Create(ticker, SecurityType.Equity, Market.USA);

            var history = _brokerage.GetPriceHistory(symbol);

            Assert.IsNotNull(history);
        }

        [TestCase("AAPL", Resolution.Minute)]
        [TestCase("AAPL", Resolution.Hour)]
        [TestCase("AAPL", Resolution.Daily)]
        public void TestHistoryProvider_GetHistory(string ticker, Resolution resolution)
        {
            var symbol = Symbol.Create(ticker, SecurityType.Equity, Market.USA);

            DateTime startDateTime = DateTime.UtcNow.AddDays(-2.0);
            DateTime endDateTime = DateTime.UtcNow;

            var historyRequest = new HistoryRequest(
                new SubscriptionDataConfig(typeof(TradeBar), symbol, resolution, DateTimeZone.Utc, DateTimeZone.Utc, true, true, true), 
                SecurityExchangeHours.AlwaysOpen(DateTimeZone.Utc), 
                startDateTime,
                endDateTime);

            var histories = _brokerage.GetHistory(historyRequest);

            Assert.IsNotEmpty(histories);

            var history = histories.FirstOrDefault();

            Assert.IsNotNull(history);

            Assert.Greater(history.Price, 0m);
            Assert.Greater(history.Value, 0m);
            Assert.That(history.Symbol.Value, Is.EqualTo(ticker).NoClip);

            Assert.IsTrue(history.DataType == MarketDataType.TradeBar);

            TradeBar historyBar = (TradeBar)history;

            Assert.Greater(historyBar.Low, 0m);
            Assert.Greater(historyBar.Close, 0m);
            Assert.Greater(historyBar.High, 0m);
            Assert.Greater(historyBar.Open, 0m);

        }

        [TestCase("AAPL")] // EQUITY
        [TestCase("VGHAX")] // MUTUAL_FUND
        public void GetQuote(string symbol)
        {
            var quoteData = _brokerage.GetQuote(symbol);

            Assert.IsNotEmpty(quoteData.Symbol);
        }

        [TestCase("AAPL", "VGHAX")] // EQUITY, MUTUAL_FUND
        public void GetQuotes(string symbol1, string symbol2)
        {
            var quoteData = _brokerage.GetQuotes(symbol1, symbol2);

            Assert.That(quoteData.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetSignInUrl()
        {
            string expected = $"https://auth.tdameritrade.com/auth?response_type=code&redirect_uri=http%3a%2f%2flocalhost&client_id={_consumerKey}%40AMER.OAUTHAP";

            var url = _brokerage.GetSignInUrl();

            Assert.IsNotNull(url);
            Assert.That(url, Is.EqualTo(expected));
        }

        [Test]
        public async Task GetRefreshToken()
        {
            var res = await _brokerage.PostAccessToken(GrantType.AuthorizationCode, _codeFromUrl);

            Assert.IsNotNull(res);
        }

        [Test]
        public async Task UpdateAccessToken()
        {
            var res = await _brokerage.PostAccessToken(GrantType.RefreshToken, string.Empty);
            
            Assert.IsNotNull(res);
        }

        [Test]
        public void GetAccounts()
        {
            var res = _brokerage.GetAccounts();
                
            Assert.IsNotNull(res);
            Assert.Greater(res[0].SecuritiesAccount.InitialBalances.CashBalance, 0);
            Assert.Greater(res[0].SecuritiesAccount.InitialBalances.Equity, 0);
            Assert.Greater(res[0].SecuritiesAccount.InitialBalances.LongStockValue, 0);

            Assert.Greater(res[0].SecuritiesAccount.CurrentBalances.CashBalance, 0);
            Assert.Greater(res[0].SecuritiesAccount.CurrentBalances.LiquidationValue, 0);
            Assert.Greater(res[0].SecuritiesAccount.CurrentBalances.BuyingPowerNonMarginableTrade, 0);

            Assert.Greater(res[0].SecuritiesAccount.ProjectedBalances.AvailableFunds, 0);
            Assert.Greater(res[0].SecuritiesAccount.ProjectedBalances.BuyingPower, 0);
            Assert.Greater(res[0].SecuritiesAccount.ProjectedBalances.AvailableFundsNonMarginableTrade, 0);
        }

    }
}
