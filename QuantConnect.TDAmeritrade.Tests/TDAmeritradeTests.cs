using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.Securities;
using NodaTime;
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeTests : BrokerageTests
    {
        protected override Symbol Symbol => Symbol.Create("AAPL", SecurityType.Equity, Market.USA);

        protected override SecurityType SecurityType => SecurityType.Equity;        

        protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            string _consumerKey = Config.Get("tdameritrade-consumer-key");
            string _callbackUrl = Config.Get("tdameritrade-callback-url");
            string _codeFromUrl = Config.Get("tdameritrade-code-from-url");
            string _refreshToken = Config.Get("tdameritrade-refresh-token");
            string _accountNumber = Config.Get("tdameritrade-account-number");

            return new TDAmeritradeBrokerage(_consumerKey, _refreshToken, _callbackUrl, _codeFromUrl, _accountNumber, null, null, new AggregationManager());
        }

        protected override bool IsAsync() => true;
        protected override bool IsCancelAsync() => true;

        protected override decimal GetAskPrice(Symbol symbol)
        {
            throw new NotImplementedException();
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

            var histories = Brokerage.GetHistory(historyRequest);

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

        [Test]
        public void GetAccountHoldings()
        {
            var res = Brokerage.GetAccountHoldings();

            Assert.IsNotNull(res);
            Assert.Greater(res.Count, 0);
        }

        [Test]
        public void GetOpenOrders()
        {
            var orders = Brokerage.GetOpenOrders();

            Assert.IsNotNull(orders);
        }

        [Ignore("Ignore to save cash")]
        [Test]
        public void PlaceOrderMarket()
        {
            var symbol = Symbols.LODE;

            var order = new MarketOrder(symbol, 1, DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        [Ignore("Ignore to save cash")]
        [Test]
        public void PlaceOrderLimit()
        {
            var symbol = Symbols.LODE;

            var price = ((TDAmeritradeBrokerage)Brokerage).GetQuote(symbol.Value).LastPrice;

            var order = new LimitOrder(symbol, 1, price + (price * 0.1m), DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        //[Ignore("Ignore to save cash")]
        [Test]
        public void PlaceOrderStopLimit()
        {
            var symbol = Symbols.LODE;

            var price = ((TDAmeritradeBrokerage)Brokerage).GetQuote(symbol.Value).LastPrice;

            var order = new StopLimitOrder(symbol, 1, price + (price * 0.1m), price + (price * 0.2m), DateTime.UtcNow);

            var isPlaceOrder = Brokerage.PlaceOrder(order);

            Assert.IsTrue(isPlaceOrder);
        }

        #region REST API

        [TestCase("037833100")] // Apple Inc. [AAPL]
        public void GetInstrumentByCUSIP(string cusip)
        {
            var instrument = ((TDAmeritradeBrokerage)Brokerage).GetInstrumentByCUSIP(cusip);

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
            var instrument = ((TDAmeritradeBrokerage)Brokerage).GetSearchInstruments(symbol, projectionType);

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

            var history = ((TDAmeritradeBrokerage)Brokerage).GetPriceHistory(symbol);

            Assert.IsNotNull(history);
        }

        [TestCase("AAPL")] // EQUITY
        [TestCase("VGHAX")] // MUTUAL_FUND
        public void GetQuote(string symbol)
        {
            var quoteData = ((TDAmeritradeBrokerage)Brokerage).GetQuote(symbol);

            Assert.IsNotEmpty(quoteData.Symbol);
        }

        [TestCase("AAPL", "VGHAX")] // EQUITY, MUTUAL_FUND
        public void GetQuotes(string symbol1, string symbol2)
        {
            var quoteData = ((TDAmeritradeBrokerage)Brokerage).GetQuotes(symbol1, symbol2);

            Assert.That(quoteData.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetSignInUrl()
        {
            string expected = $"https://auth.tdameritrade.com/auth?response_type=code&redirect_uri=http%3a%2f%2flocalhost&client_id={Config.Get("tdameritrade-consumer-key")}%40AMER.OAUTHAP";

            var url = ((TDAmeritradeBrokerage)Brokerage).GetSignInUrl();

            Assert.IsNotNull(url);
            Assert.That(url, Is.EqualTo(expected));
        }

        [Ignore("You have to update your code from url in config to complete this test, seeAlso: GetSignInUrl() Test")]
        [Test]
        public void GetRefreshToken()
        {
            var res = ((TDAmeritradeBrokerage)Brokerage).PostAccessToken(GrantType.AuthorizationCode, Config.Get("tdameritrade-code-from-url"));

            Assert.IsNotNull(res);
        }

        [Test]
        public void UpdateAccessToken()
        {
            var res = ((TDAmeritradeBrokerage)Brokerage).PostAccessToken(GrantType.RefreshToken, string.Empty);

            Assert.IsNotNull(res);
        }

        [Test]
        public void GetAccounts()
        {
            var res = ((TDAmeritradeBrokerage)Brokerage).GetAccounts();

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
        public void GetAccount()
        {
            var res = ((TDAmeritradeBrokerage)Brokerage).GetAccount(Config.Get("tdameritrade-account-number"));

            Assert.IsNotNull(res);
            Assert.Greater(res.SecuritiesAccount.InitialBalances.CashBalance, 0);
            Assert.Greater(res.SecuritiesAccount.InitialBalances.Equity, 0);
            Assert.Greater(res.SecuritiesAccount.InitialBalances.LongStockValue, 0);

            Assert.Greater(res.SecuritiesAccount.CurrentBalances.CashBalance, 0);
            Assert.Greater(res.SecuritiesAccount.CurrentBalances.LiquidationValue, 0);
            Assert.Greater(res.SecuritiesAccount.CurrentBalances.BuyingPowerNonMarginableTrade, 0);

            Assert.Greater(res.SecuritiesAccount.ProjectedBalances.AvailableFunds, 0);
            Assert.Greater(res.SecuritiesAccount.ProjectedBalances.BuyingPower, 0);
            Assert.Greater(res.SecuritiesAccount.ProjectedBalances.AvailableFundsNonMarginableTrade, 0);
        }

        [Test]
        public void GetOrdersByPath()
        {
            var currentDay = DateTime.Now;
            var order = ((TDAmeritradeBrokerage)Brokerage).GetOrdersByPath(10, toEnteredTime: currentDay).First();

            Assert.IsNotNull(order);
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
            var order = ((TDAmeritradeBrokerage)Brokerage).GetOrder(orderNumber);

            Assert.IsNotNull(order);
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
            var userPrincipals = ((TDAmeritradeBrokerage)Brokerage).GetUserPrincipals();

            Assert.IsNotNull(userPrincipals);
        }

        [TestCase("9556056706")]
        [TestCase("9556720217")]
        [TestCase("9558159438")]
        [TestCase("9558160576")]
        public void CancelOrder(string orderNumber)
        {
            var res = ((TDAmeritradeBrokerage)Brokerage).CancelOrder(orderNumber);

            Assert.IsFalse(res);
        }

        [TestCase(MarketType.OPTION)]
        [TestCase(MarketType.BOND)]
        [TestCase(MarketType.EQUITY)]
        [TestCase(MarketType.FUTURE)]
        [TestCase(MarketType.FOREX)]
        public void GetHoursForSingleMarket(MarketType marketType)
        {
            var hoursOption = ((TDAmeritradeBrokerage)Brokerage).GetHoursForSingleMarket(marketType);

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
            var hoursOption = ((TDAmeritradeBrokerage)Brokerage).GetHoursForMultipleMarkets(marketTypes);

            Assert.IsNotNull(hoursOption);
            Assert.That(hoursOption.Count, Is.EqualTo(marketTypes.Length));
        }

        [TestCase(IndexMoverType.DJI, DirectionType.NoValue, ChangeType.NoValue)]
        [TestCase(IndexMoverType.COMPX, DirectionType.NoValue, ChangeType.NoValue)]
        [TestCase(IndexMoverType.COMPX, DirectionType.Down, ChangeType.Percent)]
        [TestCase(IndexMoverType.SPX_X, DirectionType.NoValue, ChangeType.NoValue)]
        [TestCase(IndexMoverType.SPX_X, DirectionType.Up, ChangeType.Value)]
        public void GetMovers(IndexMoverType indexMoverType, DirectionType directionType, ChangeType changeType)
        {
            var mover = ((TDAmeritradeBrokerage)Brokerage).GetMovers(indexMoverType, directionType, changeType);

            Assert.IsNotNull(mover);
            Assert.Greater(mover.Count, 0);
        }

        [TestCase(TransactionType.TRADE, null)]
        [TestCase(TransactionType.TRADE, "BLZE")]
        [TestCase(TransactionType.BUY_ONLY, null)]
        public void GetTransaction(TransactionType transactionType, string symbol)
        {
            var transactions = ((TDAmeritradeBrokerage)Brokerage).GetTransactions(transactionType: transactionType, symbol: symbol);

            Assert.IsNotNull(transactions);
            Assert.Greater(transactions.Count(), 0);

            Assert.IsNotEmpty(transactions.First().Instrument.Symbol);
            Assert.IsNotEmpty(transactions.First().Instrument.Cusip);
        }

        #endregion


    }
}
