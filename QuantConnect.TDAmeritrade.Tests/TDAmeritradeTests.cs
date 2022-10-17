using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.Securities;
using NodaTime;
using QuantConnect.Data.Market;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels;

namespace QuantConnect.TDAmeritrade.Tests
{
    public class TDAmeritradeTests
    {
        private Application.TDAmeritrade _brokerage;

        private readonly string _consumerKey = Config.Get("tdameritrade-consumer-key");
        private readonly string _callbackUrl = Config.Get("tdameritrade-callback-url");
        private readonly string _codeFromUrl = Config.Get("tdameritrade-code-from-url");
        private readonly string _refreshToken = Config.Get("tdameritrade-refresh-token");
        private readonly string _accountNumber = Config.Get("tdameritrade-account-number");

        [OneTimeSetUp]
        public void Setup() => _brokerage = new Application.TDAmeritrade(_consumerKey, _refreshToken, _callbackUrl, _codeFromUrl, _accountNumber, null);

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

        [Ignore("You have to update your code from url in config to complete this test, seeAlso: GetSignInUrl() Test")]
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

        [Test]
        public void GetOrdersByPath()
        {
            var currentDay = DateTime.Now;
            var order = _brokerage.GetOrdersByPath(10, toEnteredTime: currentDay).First();

            Assert.IsNotNull(order);
            Assert.IsNotEmpty(order.Status);
            Assert.Greater(order.AccountId, 0);
            Assert.GreaterOrEqual(order.Price, 0);
            Assert.IsNotEmpty(order.OrderType);
            Assert.IsNotNull(order.OrderLegCollections);
            Assert.Greater(order.OrderLegCollections[0].Quantity, 0);
            Assert.Greater(order.OrderLegCollections[0].LegId, 0);
            Assert.IsNotEmpty(order.OrderLegCollections[0].Instrument.Cusip);
            Assert.IsNotEmpty(order.OrderLegCollections[0].Instrument.Symbol);
        }

        [TestCase("9556056706")]
        public void GetOrdersById(string orderNumber)
        {
            var order = _brokerage.GetOrder(orderNumber);

            Assert.IsNotNull(order);
            Assert.IsNotEmpty(order.Status);
            Assert.Greater(order.AccountId, 0);
            Assert.Greater(order.Price, 0);
            Assert.IsNotEmpty(order.OrderType);

            Assert.IsNotNull(order.OrderLegCollections);
            Assert.Greater(order.OrderLegCollections[0].Quantity, 0);
            Assert.Greater(order.OrderLegCollections[0].LegId, 0);
            Assert.IsNotEmpty(order.OrderLegCollections[0].Instrument.Cusip);
            Assert.IsNotEmpty(order.OrderLegCollections[0].Instrument.Symbol);
        }

        [Test]
        public void GetUserPrincipals()
        {
            var userPrincipals = _brokerage.GetUserPrincipals();

            Assert.IsNotNull(userPrincipals);
        }

        [TestCase("9556056706")]
        [TestCase("9556720217")]
        [TestCase("9558159438")]
        [TestCase("9558160576")]
        public void CancelOrder(string orderNumber)
        {
            var res = _brokerage.CancelOrder(orderNumber);

            Assert.IsFalse(res);
        }

        [Test]
        public void GetOpenOrders()
        {
            var orders = _brokerage.GetOpenOrders();

            Assert.IsNotNull(orders);
        }

        [TestCase(MarketType.OPTION)]
        [TestCase(MarketType.BOND)]
        [TestCase(MarketType.EQUITY)]
        [TestCase(MarketType.FUTURE)]
        [TestCase(MarketType.FOREX)]
        public void GetHoursForSingleMarket(MarketType marketType)
        {
            var hoursOption = _brokerage.GetHoursForSingleMarket(marketType);

            Assert.IsNotNull(hoursOption);

            var market = hoursOption[hoursOption.Keys.First()];

            Assert.IsNotEmpty(market.Category);
            Assert.IsNotEmpty(market.Date);
            Assert.IsNotEmpty(market.Exchange);
            Assert.IsNotEmpty(market.MarketType);
            Assert.IsNotEmpty(market.Product);
            Assert.IsNotEmpty(market.ProductName);

            if (market.SessionHours != null)
            {
                Assert.IsNotEmpty(market.SessionHours[market.SessionHours.Keys.First()][0].Start);
                Assert.IsNotEmpty(market.SessionHours[market.SessionHours.Keys.First()][0].End);
            }
        }

        [TestCase(MarketType.OPTION, MarketType.BOND)]
        [TestCase(MarketType.FUTURE, MarketType.OPTION, MarketType.BOND, MarketType.FOREX, MarketType.EQUITY)]
        public void GetHoursForMultipleMarkets(params MarketType[] marketTypes)
        {
            var hoursOption = _brokerage.GetHoursForMultipleMarkets(marketTypes);

            Assert.IsNotNull(hoursOption);
            Assert.That(hoursOption.Count, Is.EqualTo(marketTypes.Length));
        }

        [Ignore("Market hasn't completed yet")]
        [TestCase(OrderType.Market, InstructionType.Buy, 1, "BLZE")]
        public void PostOrderMarket(OrderType orderType, InstructionType instructionType, decimal quantity, string symbol)
        {
            var session = SessionType.Normal;
            var duration = DurationType.Day;
            var orderStrategyType = OrderStrategyType.Single;

            PlaceOrderLegCollectionModel orderLegCollectionModel = new(instructionType, quantity, new InstrumentPlaceOrderModel(symbol, "EQUITY"));
            //OrderLegCollectionModel orderLegCollectionModel2 = new(InstructionType.Buy, 10, new InstrumentPlaceOrderModel("CBTRP", "EQUITY"));

            List<PlaceOrderLegCollectionModel> orderLegCollectionModels = new();
            orderLegCollectionModels.Add(orderLegCollectionModel);
            //orderLegCollectionModels.Add(orderLegCollectionModel2);

            var order = _brokerage.PostPlaceOrder(orderType, session, duration, orderStrategyType, orderLegCollectionModels);

            Assert.IsNotNull(order);
        }

        [Ignore("Limit hasn't completed yet")]
        [TestCase(OrderType.Limit, 0.0003, InstructionType.Buy, 1, "CBTRP")]
        [TestCase(OrderType.Limit, 4.6, InstructionType.Sell, 1, "BLZE")]
        [TestCase(OrderType.Limit, 4.55, InstructionType.Buy, 1, "BLZE")]
        public void PostOrderLimit(OrderType orderType, decimal price, InstructionType instructionType, decimal quantity, string symbol)
        {
            var session = SessionType.Normal;
            var duration = DurationType.Day;
            var orderStrategyType = OrderStrategyType.Single;
            var complexOrderStrategyType = ComplexOrderStrategyType.None;

            PlaceOrderLegCollectionModel orderLegCollectionModel = new(instructionType, quantity, new InstrumentPlaceOrderModel(symbol, "EQUITY"));

            List<PlaceOrderLegCollectionModel> orderLegCollectionModels = new();
            orderLegCollectionModels.Add(orderLegCollectionModel);

            var order = _brokerage.PostPlaceOrder(orderType, session, duration, orderStrategyType, orderLegCollectionModels, complexOrderStrategyType, price);

            Assert.IsNotNull(order);
        }

    }
}
