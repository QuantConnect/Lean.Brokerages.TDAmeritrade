using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct AuthorizationsModel
    {
        [JsonProperty(PropertyName = "apex")]
        public bool Apex { get; set; }

        [JsonProperty(PropertyName = "levelTwoQuotes")]
        public bool LevelTwoQuotes { get; set; }

        [JsonProperty(PropertyName = "stockTrading")]
        public bool StockTrading { get; set; }

        [JsonProperty(PropertyName = "marginTrading")]
        public bool MarginTrading { get; set; }

        [JsonProperty(PropertyName = "streamingNews")]
        public bool StreamingNews { get; set; }

        /// <summary>
        /// 'COVERED' or 'FULL' or 'LONG' or 'SPREAD' or 'NONE'
        /// </summary>
        [JsonProperty(PropertyName = "optionTradingLevel")]
        public string OptionTradingLevel { get; set; }

        [JsonProperty(PropertyName = "streamerAccess")]
        public bool StreamerAccess { get; set; }

        [JsonProperty(PropertyName = "advancedMargin")]
        public bool AdvancedMargin { get; set; }

        [JsonProperty(PropertyName = "scottradeAccount")]
        public bool ScottradeAccount { get; set; }

        [JsonProperty(PropertyName = "autoPositionEffect")]
        public bool AutoPositionEffect { get; set; }
    }
}
