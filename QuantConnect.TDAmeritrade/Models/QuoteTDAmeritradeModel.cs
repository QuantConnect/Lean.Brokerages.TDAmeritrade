using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class QuoteTDAmeritradeModel
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "closePrice")]
        public decimal ClosePrice { get; set; }

        [JsonProperty(PropertyName = "netChange")]
        public decimal NetChange { get; set; }

        [JsonProperty(PropertyName = "totalVolume")]
        public int TotalVolume { get; set; }

        [JsonProperty(PropertyName = "tradeTimeInLong")]
        public long TradeTimeInLong { get; set; }

        [JsonProperty(PropertyName = "exchange")]
        public string Exchange { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "exchangeName")]
        public string ExchangeName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "digits")]
        public int Digits { get; set; }

        [JsonProperty(PropertyName = "52WkHigh")]
        public decimal _52WkHigh { get; set; }

        [JsonProperty(PropertyName = "52WkLow")]
        public decimal _52WkLow { get; set; }

        [JsonProperty(PropertyName = "nAV")]
        public decimal NAV { get; set; }

        [JsonProperty(PropertyName = "peRatio")]
        public decimal PeRatio { get; set; }

        [JsonProperty(PropertyName = "divAmount")]
        public decimal DivAmount { get; set; }

        [JsonProperty(PropertyName = "divYield")]
        public decimal DivYield { get; set; }

        [JsonProperty(PropertyName = "divDate")]
        public string DivDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "securityStatus")]
        public string SecurityStatus { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "bidPriceInDouble")]
        public decimal BidPriceInDouble { get; set; }

        [JsonProperty(PropertyName = "askPriceInDouble")]
        public decimal AskPriceInDouble { get; set; }

        [JsonProperty(PropertyName = "lastPriceInDouble")]
        public decimal LastPriceInDouble { get; set; }

        [JsonProperty(PropertyName = "bidId")]
        public string BidId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "askId")]
        public string AskId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "highPriceInDouble")]
        public decimal HighPriceInDouble { get; set; }

        [JsonProperty(PropertyName = "lowPriceInDouble")]
        public decimal LowPriceInDouble { get; set; }

        [JsonProperty(PropertyName = "closePriceInDouble")]
        public decimal ClosePriceInDouble { get; set; }

        [JsonProperty(PropertyName = "lastId")]
        public string LastId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "openPriceInDouble")]
        public decimal OpenPriceInDouble { get; set; }

        [JsonProperty(PropertyName = "changeInDouble")]
        public decimal ChangeInDouble { get; set; }

        [JsonProperty(PropertyName = "futurePercentChange")]
        public decimal FuturePercentChange { get; set; }

        [JsonProperty(PropertyName = "openInterest")]
        public decimal OpenInterest { get; set; }

        [JsonProperty(PropertyName = "mark")]
        public decimal Mark { get; set; }

        [JsonProperty(PropertyName = "tick")]
        public decimal Tick { get; set; }

        [JsonProperty(PropertyName = "tickAmount")]
        public decimal TickAmount { get; set; }

        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "futurePriceFormat")]
        public string FuturePriceFormat { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "futureTradingHours")]
        public string FutureTradingHours { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "futureIsTradable")]
        public bool FutureIsTradable { get; set; }

        [JsonProperty(PropertyName = "futureMultiplier")]
        public decimal FutureMultiplier { get; set; }

        [JsonProperty(PropertyName = "futureIsActive")]
        public bool FutureIsActive { get; set; }

        [JsonProperty(PropertyName = "futureSettlementPrice")]
        public decimal FutureSettlementPrice { get; set; }

        [JsonProperty(PropertyName = "futureActiveSymbol")]
        public string FutureActiveSymbol { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "futureExpirationDate")]
        public string FutureExpirationDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "netChangeInDouble")]
        public decimal NetChangeInDouble { get; set; }

        [JsonProperty(PropertyName = "volatility")]
        public decimal Volatility { get; set; }

        [JsonProperty(PropertyName = "moneyIntrinsicValueInDouble")]
        public decimal MoneyIntrinsicValueInDouble { get; set; }

        [JsonProperty(PropertyName = "multiplierInDouble")]
        public decimal MultiplierInDouble { get; set; }

        [JsonProperty(PropertyName = "strikePriceInDouble")]
        public decimal StrikePriceInDouble { get; set; }

        [JsonProperty(PropertyName = "contractType")]
        public string ContractType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "underlying")]
        public string Underlying { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "timeValueInDouble")]
        public decimal TimeValueInDouble { get; set; }

        [JsonProperty(PropertyName = "deltaInDouble")]
        public decimal DeltaInDouble { get; set; }

        [JsonProperty(PropertyName = "gammaInDouble")]
        public decimal GammaInDouble { get; set; }

        [JsonProperty(PropertyName = "thetaInDouble")]
        public decimal ThetaInDouble { get; set; }

        [JsonProperty(PropertyName = "vegaInDouble")]
        public decimal VegaInDouble { get; set; }

        [JsonProperty(PropertyName = "rhoInDouble")]
        public decimal RhoInDouble { get; set; }

        //[JsonProperty(PropertyName = "futureExpirationDate")]
        //public int FutureExpirationDate { get; set; }

        [JsonProperty(PropertyName = "expirationType")]
        public string ExpirationType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "exerciseType")]
        public string ExerciseType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "inTheMoney")]
        public bool inTheMoney { get; set; }

        [JsonProperty(PropertyName = "lastPrice")]
        public decimal LastPrice { get; set; }

        [JsonProperty(PropertyName = "openPrice")]
        public decimal OpenPrice { get; set; }

        [JsonProperty(PropertyName = "highPrice")]
        public decimal HighPrice { get; set; }

        [JsonProperty(PropertyName = "lowPrice")]
        public decimal LowPrice { get; set; }

        [JsonProperty(PropertyName = "bidPrice")]
        public decimal BidPrice { get; set; }

        [JsonProperty(PropertyName = "bidSize")]
        public int BidSize { get; set; }

        [JsonProperty(PropertyName = "askPrice")]
        public decimal AskPrice { get; set; }

        [JsonProperty(PropertyName = "askSize")]
        public int AskSize { get; set; }

        [JsonProperty(PropertyName = "lastSize")]
        public int LastSize { get; set; }

        [JsonProperty(PropertyName = "quoteTimeInLong")]
        public long QuoteTimeInLong { get; set; }

        [JsonProperty(PropertyName = "moneyIntrinsicValue")]
        public decimal MoneyIntrinsicValue { get; set; }

        [JsonProperty(PropertyName = "multiplier")]
        public decimal Multiplier { get; set; }

        [JsonProperty(PropertyName = "strikePrice")]
        public decimal StrikePrice { get; set; }

        [JsonProperty(PropertyName = "timeValue")]
        public decimal TimeValue { get; set; }

        [JsonProperty(PropertyName = "deliverables")]
        public string Deliverables { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "delta")]
        public decimal Delta { get; set; }

        [JsonProperty(PropertyName = "gamma")]
        public decimal Gamma { get; set; }

        [JsonProperty(PropertyName = "theta")]
        public decimal Theta { get; set; }

        [JsonProperty(PropertyName = "vega")]
        public decimal Vega { get; set; }

        [JsonProperty(PropertyName = "rho")]
        public decimal Rho { get; set; }

        [JsonProperty(PropertyName = "theoreticalOptionValue")]
        public decimal TheoreticalOptionValue { get; set; }

        [JsonProperty(PropertyName = "underlyingPrice")]
        public decimal UnderlyingPrice { get; set; }

        [JsonProperty(PropertyName = "uvExpirationType")]
        public string UvExpirationType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "settlementType")]
        public string SettlementType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "percentChange")]
        public decimal PercentChange { get; set; }

        [JsonProperty(PropertyName = "tradingHours")]
        public string TradingHours { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "isTradable")]
        public bool IsTradable { get; set; }

        [JsonProperty(PropertyName = "marketMaker")]
        public string MarketMaker { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "52WkHighInDouble")]
        public decimal _52WkHighInDouble { get; set; }

        [JsonProperty(PropertyName = "52WkLowInDouble")]
        public decimal _52WkLowInDouble { get; set; }

        [JsonProperty(PropertyName = "marginable")]
        public bool Marginable { get; set; }

        [JsonProperty(PropertyName = "shortable")]
        public bool Shortable { get; set; }

        [JsonProperty(PropertyName = "regularMarketLastPrice")]
        public decimal RegularMarketLastPrice { get; set; }

        [JsonProperty(PropertyName = "regularMarketLastSize")]
        public int RegularMarketLastSize { get; set; }

        [JsonProperty(PropertyName = "regularMarketNetChange")]
        public decimal RegularMarketNetChange { get; set; }

        [JsonProperty(PropertyName = "regularMarketTradeTimeInLong")]
        public long RegularMarketTradeTimeInLong { get; set; }
    }
}
