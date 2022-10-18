using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class AccountModel
    {
        [JsonProperty(PropertyName = "securitiesAccount")]
        public SecuritiesAccountModel SecuritiesAccount { get; set; } = new();
    }

    public class SecuritiesAccountModel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "roundTrips")]
        public int RoundTrips { get; set; }

        [JsonProperty(PropertyName = "isDayTrader")]
        public bool IsDayTrader { get; set; }

        [JsonProperty(PropertyName = "isClosingOnlyRestricted")]
        public bool IsClosingOnlyRestricted { get; set; }

        [JsonProperty(PropertyName = "positions")]
        public List<Positions> Positions { get; set; } = new();

        [JsonProperty(PropertyName = "initialBalances")]
        public InitialBalances InitialBalances { get; set; } = new();

        [JsonProperty(PropertyName = "currentBalances")]
        public CurrentBalances CurrentBalances { get; set; } = new();

        [JsonProperty(PropertyName = "projectedBalances")]
        public ProjectedBalances ProjectedBalances { get; set; } = new();
    }

    public class Positions
    {
        [JsonProperty(PropertyName = "shortQuantity")]
        public decimal ShortQuantity { get; set; }

        [JsonProperty(PropertyName = "averagePrice")]
        public decimal AveragePrice { get; set; }

        [JsonProperty(PropertyName = "currentDayCost")]
        public decimal CurrentDayCost { get; set; }

        [JsonProperty(PropertyName = "currentDayProfitLoss")]
        public decimal CurrentDayProfitLoss { get; set; }

        [JsonProperty(PropertyName = "currentDayProfitLossPercentage")]
        public decimal CurrentDayProfitLossPercentage { get; set; }

        [JsonProperty(PropertyName = "longQuantity")]
        public decimal LongQuantity { get; set; }

        [JsonProperty(PropertyName = "settledLongQuantity")]
        public decimal SettledLongQuantity { get; set; }

        [JsonProperty(PropertyName = "settledShortQuantity")]
        public decimal SettledShortQuantity { get; set; }

        [JsonProperty(PropertyName = "instrument")]
        public Instrument ProjectedBalances { get; set; } = new();

        [JsonProperty(PropertyName = "marketValue")]
        public decimal MarketValue { get; set; }

        [JsonProperty(PropertyName = "maintenanceRequirement")]
        public decimal MaintenanceRequirement { get; set; }

        [JsonProperty(PropertyName = "previousSessionLongQuantity")]
        public decimal PreviousSessionLongQuantity { get; set; }
    }

    public class Instrument
    {
        [JsonProperty(PropertyName = "assetType")]
        public string AssetType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "cusip")]
        public string Cusip { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;

    }

    public class InitialBalances
    {
        [JsonProperty(PropertyName = "accruedInterest")]
        public decimal AccruedInterest { get; set; }

        [JsonProperty(PropertyName = "availableFundsNonMarginableTrade")]
        public decimal AvailableFundsNonMarginableTrade { get; set; }

        [JsonProperty(PropertyName = "bondValue")]
        public decimal BondValue { get; set; }

        [JsonProperty(PropertyName = "buyingPower")]
        public decimal BuyingPower { get; set; }

        [JsonProperty(PropertyName = "cashBalance")]
        public decimal CashBalance { get; set; }

        [JsonProperty(PropertyName = "cashAvailableForTrading")]
        public decimal CashAvailableForTrading { get; set; }

        [JsonProperty(PropertyName = "cashReceipts")]
        public decimal CashReceipts { get; set; }

        [JsonProperty(PropertyName = "dayTradingBuyingPower")]
        public decimal DayTradingBuyingPower { get; set; }

        [JsonProperty(PropertyName = "dayTradingBuyingPowerCall")]
        public decimal DayTradingBuyingPowerCall { get; set; }

        [JsonProperty(PropertyName = "dayTradingEquityCall")]
        public decimal DayTradingEquityCall { get; set; }

        [JsonProperty(PropertyName = "equity")]
        public decimal Equity { get; set; }

        [JsonProperty(PropertyName = "equityPercentage")]
        public decimal EquityPercentage { get; set; }

        [JsonProperty(PropertyName = "liquidationValue")]
        public decimal LiquidationValue { get; set; }

        [JsonProperty(PropertyName = "longMarginValue")]
        public decimal LongMarginValue { get; set; }

        [JsonProperty(PropertyName = "longOptionMarketValue")]
        public decimal LongOptionMarketValue { get; set; }

        [JsonProperty(PropertyName = "longStockValue")]
        public decimal LongStockValue { get; set; }

        [JsonProperty(PropertyName = "maintenanceCall")]
        public decimal MaintenanceCall { get; set; }

        [JsonProperty(PropertyName = "maintenanceRequirement")]
        public decimal MaintenanceRequirement { get; set; }

        [JsonProperty(PropertyName = "margin")]
        public decimal Margin { get; set; }

        [JsonProperty(PropertyName = "marginEquity")]
        public decimal MarginEquity { get; set; }

        [JsonProperty(PropertyName = "moneyMarketFund")]
        public decimal MoneyMarketFund { get; set; }

        [JsonProperty(PropertyName = "mutualFundValue")]
        public decimal MutualFundValue { get; set; }

        [JsonProperty(PropertyName = "regTCall")]
        public decimal RegTCall { get; set; }

        [JsonProperty(PropertyName = "shortMarginValue")]
        public decimal ShortMarginValue { get; set; }

        [JsonProperty(PropertyName = "shortOptionMarketValue")]
        public decimal ShortOptionMarketValue { get; set; }

        [JsonProperty(PropertyName = "shortStockValue")]
        public decimal ShortStockValue { get; set; }

        [JsonProperty(PropertyName = "totalCash")]
        public decimal TotalCash { get; set; }

        [JsonProperty(PropertyName = "isInCall")]
        public bool IsInCall { get; set; }

        [JsonProperty(PropertyName = "pendingDeposits")]
        public decimal PendingDeposits { get; set; }

        [JsonProperty(PropertyName = "marginBalance")]
        public decimal MarginBalance { get; set; }

        [JsonProperty(PropertyName = "shortBalance")]
        public decimal ShortBalance { get; set; }

        [JsonProperty(PropertyName = "accountValue")]
        public decimal AccountValue { get; set; }
    }

    public class CurrentBalances
    {
        [JsonProperty(PropertyName = "accruedInterest")]
        public decimal AccruedInterest { get; set; }

        [JsonProperty(PropertyName = "cashBalance")]
        public decimal CashBalance { get; set; }

        [JsonProperty(PropertyName = "cashReceipts")]
        public decimal CashReceipts { get; set; }

        [JsonProperty(PropertyName = "longOptionMarketValue")]
        public decimal LongOptionMarketValue { get; set; }

        [JsonProperty(PropertyName = "liquidationValue")]
        public decimal LiquidationValue { get; set; }

        [JsonProperty(PropertyName = "longMarketValue")]
        public decimal LongMarketValue { get; set; }

        [JsonProperty(PropertyName = "moneyMarketFund")]
        public decimal MoneyMarketFund { get; set; }

        [JsonProperty(PropertyName = "savings")]
        public decimal Savings { get; set; }

        [JsonProperty(PropertyName = "shortMarketValue")]
        public decimal ShortMarketValue { get; set; }

        [JsonProperty(PropertyName = "pendingDeposits")]
        public decimal PendingDeposits { get; set; }

        [JsonProperty(PropertyName = "availableFunds")]
        public decimal AvailableFunds { get; set; }

        [JsonProperty(PropertyName = "availableFundsNonMarginableTrade")]
        public decimal AvailableFundsNonMarginableTrade { get; set; }

        [JsonProperty(PropertyName = "buyingPower")]
        public decimal BuyingPower { get; set; }

        [JsonProperty(PropertyName = "buyingPowerNonMarginableTrade")]
        public decimal BuyingPowerNonMarginableTrade { get; set; }

        [JsonProperty(PropertyName = "dayTradingBuyingPower")]
        public decimal DayTradingBuyingPower { get; set; }

        [JsonProperty(PropertyName = "equity")]
        public decimal Equity { get; set; }

        [JsonProperty(PropertyName = "equityPercentage")]
        public decimal EquityPercentage { get; set; }

        [JsonProperty(PropertyName = "longMarginValue")]
        public decimal LongMarginValue { get; set; }

        [JsonProperty(PropertyName = "maintenanceCall")]
        public decimal MaintenanceCall { get; set; }

        [JsonProperty(PropertyName = "maintenanceRequirement")]
        public decimal MaintenanceRequirement { get; set; }

        [JsonProperty(PropertyName = "marginBalance")]
        public decimal MarginBalance { get; set; }

        [JsonProperty(PropertyName = "regTCall")]
        public decimal RegTCall { get; set; }

        [JsonProperty(PropertyName = "shortBalance")]
        public decimal ShortBalance { get; set; }

        [JsonProperty(PropertyName = "shortMarginValue")]
        public decimal ShortMarginValue { get; set; }

        [JsonProperty(PropertyName = "shortOptionMarketValue")]
        public decimal ShortOptionMarketValue { get; set; }

        [JsonProperty(PropertyName = "sma")]
        public decimal Sma { get; set; }

        [JsonProperty(PropertyName = "mutualFundValue")]
        public decimal MutualFundValue { get; set; }

        [JsonProperty(PropertyName = "bondValue")]
        public decimal BondValue { get; set; }
    }

    public class ProjectedBalances
    {
        [JsonProperty(PropertyName = "availableFunds")]
        public decimal AvailableFunds { get; set; }

        [JsonProperty(PropertyName = "availableFundsNonMarginableTrade")]
        public decimal AvailableFundsNonMarginableTrade { get; set; }

        [JsonProperty(PropertyName = "buyingPower")]
        public decimal BuyingPower { get; set; }

        [JsonProperty(PropertyName = "dayTradingBuyingPower")]
        public decimal DayTradingBuyingPower { get; set; }

        [JsonProperty(PropertyName = "dayTradingBuyingPowerCall")]
        public decimal DayTradingBuyingPowerCall { get; set; }

        [JsonProperty(PropertyName = "maintenanceCall")]
        public decimal MaintenanceCall { get; set; }

        [JsonProperty(PropertyName = "regTCall")]
        public decimal RegTCall { get; set; }

        [JsonProperty(PropertyName = "isInCall")]
        public bool IsInCall { get; set; }

        [JsonProperty(PropertyName = "stockBuyingPower")]
        public decimal StockBuyingPower { get; set; }
    }
}
