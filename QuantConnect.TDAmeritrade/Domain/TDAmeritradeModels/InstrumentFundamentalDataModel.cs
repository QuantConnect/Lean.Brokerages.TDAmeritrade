using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public class InstrumentFundamentalDataModel
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "high52")]
        public decimal High52 { get; set; }

        [JsonProperty(PropertyName = "low52")]
        public decimal Low52 { get; set; }

        [JsonProperty(PropertyName = "dividendAmount")]
        public decimal DividendAmount { get; set; }

        [JsonProperty(PropertyName = "dividendYield")]
        public decimal DividendYield { get; set; }

        [JsonProperty(PropertyName = "dividendDate")]
        public string DividendDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "peRatio")]
        public decimal PeRatio { get; set; }

        [JsonProperty(PropertyName = "pegRatio")]
        public decimal PegRatio { get; set; }

        [JsonProperty(PropertyName = "pbRatio")]
        public decimal PbRatio { get; set; }

        [JsonProperty(PropertyName = "prRatio")]
        public decimal PrRatio { get; set; }

        [JsonProperty(PropertyName = "pcfRatio")]
        public decimal PcfRatio { get; set; }

        [JsonProperty(PropertyName = "grossMarginTTM")]
        public decimal GrossMarginTTM { get; set; }

        [JsonProperty(PropertyName = "grossMarginMRQ")]
        public decimal GrossMarginMRQ { get; set; }

        [JsonProperty(PropertyName = "netProfitMarginTTM")]
        public decimal NetProfitMarginTTM { get; set; }

        [JsonProperty(PropertyName = "netProfitMarginMRQ")]
        public decimal NetProfitMarginMRQ { get; set; }

        [JsonProperty(PropertyName = "operatingMarginTTM")]
        public decimal OperatingMarginTTM { get; set; }

        [JsonProperty(PropertyName = "operatingMarginMRQ")]
        public decimal OperatingMarginMRQ { get; set; }

        [JsonProperty(PropertyName = "returnOnEquity")]
        public decimal ReturnOnEquity { get; set; }

        [JsonProperty(PropertyName = "returnOnAssets")]
        public decimal ReturnOnAssets { get; set; }

        [JsonProperty(PropertyName = "returnOnInvestment")]
        public decimal ReturnOnInvestment { get; set; }

        [JsonProperty(PropertyName = "quickRatio")]
        public decimal QuickRatio { get; set; }

        [JsonProperty(PropertyName = "currentRatio")]
        public decimal CurrentRatio { get; set; }

        [JsonProperty(PropertyName = "interestCoverage")]
        public decimal InterestCoverage { get; set; }

        [JsonProperty(PropertyName = "totalDebtToCapital")]
        public decimal TotalDebtToCapital { get; set; }

        [JsonProperty(PropertyName = "ltDebtToEquity")]
        public decimal LtDebtToEquity { get; set; }

        [JsonProperty(PropertyName = "totalDebtToEquity")]
        public decimal TotalDebtToEquity { get; set; }

        [JsonProperty(PropertyName = "epsTTM")]
        public decimal EpsTTM { get; set; }

        [JsonProperty(PropertyName = "epsChangePercentTTM")]
        public decimal EpsChangePercentTTM { get; set; }

        [JsonProperty(PropertyName = "epsChangeYear")]
        public decimal EpsChangeYear { get; set; }

        [JsonProperty(PropertyName = "epsChange")]
        public decimal EpsChange { get; set; }

        [JsonProperty(PropertyName = "revChangeYear")]
        public decimal RevChangeYear { get; set; }

        [JsonProperty(PropertyName = "revChangeTTM")]
        public decimal RevChangeTTM { get; set; }

        [JsonProperty(PropertyName = "revChangeIn")]
        public decimal RevChangeIn { get; set; }

        [JsonProperty(PropertyName = "sharesOutstanding")]
        public decimal SharesOutstanding { get; set; }

        [JsonProperty(PropertyName = "marketCapFloat")]
        public decimal MarketCapFloat { get; set; }

        [JsonProperty(PropertyName = "marketCap")]
        public decimal MarketCap { get; set; }

        [JsonProperty(PropertyName = "bookValuePerShare")]
        public decimal BookValuePerShare { get; set; }

        [JsonProperty(PropertyName = "shortIntToFloat")]
        public decimal ShortIntToFloat { get; set; }

        [JsonProperty(PropertyName = "shortIntDayToCover")]
        public decimal ShortIntDayToCover { get; set; }

        [JsonProperty(PropertyName = "divGrowthRate3Year")]
        public decimal DivGrowthRate3Year { get; set; }

        [JsonProperty(PropertyName = "dividendPayAmount")]
        public decimal DividendPayAmount { get; set; }

        [JsonProperty(PropertyName = "dividendPayDate")]
        public string DividendPayDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "beta")]
        public decimal Beta { get; set; }

        [JsonProperty(PropertyName = "vol1DayAvg")]
        public decimal Vol1DayAvg { get; set; }

        [JsonProperty(PropertyName = "vol10DayAvg")]
        public decimal Vol10DayAvg { get; set; }

        [JsonProperty(PropertyName = "vol3MonthAvg")]
        public decimal Vol3MonthAvg { get; set; }
    }
}
