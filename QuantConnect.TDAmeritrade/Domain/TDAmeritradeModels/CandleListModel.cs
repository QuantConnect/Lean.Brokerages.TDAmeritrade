using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    /// <summary>
    /// Model for price history for a symbol
    /// </summary>
    public class CandleListModel
    {
        [JsonProperty(PropertyName = "candles")]
        public List<CandleModel> Candles { get; set; } = new();

        [JsonProperty(PropertyName = "empty")]
        public bool Empty { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;
    }

    public class CandleModel
    {
        /// Historical Data Bar: Close
        [JsonProperty(PropertyName = "close")]
        public decimal Close;

        /// Historical Data Bar: Date
        [JsonProperty(PropertyName = "datetime")]
        public decimal DateTime;

        /// Historical Data Bar: High
        [JsonProperty(PropertyName = "high")]
        public decimal High;

        /// Historical Data Bar: Low
        [JsonProperty(PropertyName = "low")]
        public decimal Low;

        /// Historical Data Bar: Open
        [JsonProperty(PropertyName = "open")]
        public decimal Open;

        /// Historical Data Bar: Volume
        [JsonProperty(PropertyName = "volume")]
        public long Volume;
    }
}
