using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct PreferencesModel
    {
        [JsonProperty(PropertyName = "expressTrading")]
        public bool ExpressTrading { get; set; }

        [JsonProperty(PropertyName = "directOptionsRouting")]
        public bool DirectOptionsRouting { get; set; }

        [JsonProperty(PropertyName = "directEquityRouting")]
        public bool DirectEquityRouting { get; set; }

        /// <summary>
        /// 'BUY' or 'SELL' or 'BUY_TO_COVER' or 'SELL_SHORT' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "defaultEquityOrderLegInstruction")]
        public string DefaultEquityOrderLegInstruction { get; set; }

        /// <summary>
        /// 'MARKET' or 'LIMIT' or 'STOP' or 'STOP_LIMIT' or 'TRAILING_STOP' or 'MARKET_ON_CLOSE' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "defaultEquityOrderType")]
        public string DefaultEquityOrderType { get; set; }

        /// <summary>
        /// 'VALUE' or 'PERCENT' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "defaultEquityOrderPriceLinkType")]
        public string DefaultEquityOrderPriceLinkType { get; set; }

        /// <summary>
        /// 'DAY' or 'GOOD_TILL_CANCEL' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "defaultEquityOrderDuration")]
        public string DefaultEquityOrderDuration { get; set; }

        /// <summary>
        /// 'AM' or 'PM' or 'NORMAL' or 'SEAMLESS' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "defaultEquityOrderMarketSession")]
        public string DefaultEquityOrderMarketSession { get; set; }

        [JsonProperty(PropertyName = "defaultEquityQuantity")]
        public int DefaultEquityQuantity { get; set; }

        /// <summary>
        /// 'FIFO' or 'LIFO' or 'HIGH_COST' or 'LOW_COST' or 'MINIMUM_TAX' or 'AVERAGE_COST' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "mutualFundTaxLotMethod")]
        public string MutualFundTaxLotMethod { get; set; }

        /// <summary>
        /// 'FIFO' or 'LIFO' or 'HIGH_COST' or 'LOW_COST' or 'MINIMUM_TAX' or 'AVERAGE_COST' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "optionTaxLotMethod")]
        public string OptionTaxLotMethod { get; set; }

        /// <summary>
        /// 'FIFO' or 'LIFO' or 'HIGH_COST' or 'LOW_COST' or 'MINIMUM_TAX' or 'AVERAGE_COST' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "equityTaxLotMethod")]
        public string EquityTaxLotMethod { get; set; }

        /// <summary>
        /// 'TA' or 'N' or 'Y' or 'TOS' or 'NONE' or 'CC2'
        /// </summary>
        [JsonProperty(PropertyName = "defaultAdvancedToolLaunch")]
        public string DefaultAdvancedToolLaunch { get; set; }

        /// <summary>
        /// 'FIFTY_FIVE_MINUTES' or 'TWO_HOURS' or 'FOUR_HOURS' or 'EIGHT_HOURS'
        /// </summary>
        [JsonProperty(PropertyName = "authTokenTimeout")]
        public string AuthTokenTimeout { get; set; }
    }
}
