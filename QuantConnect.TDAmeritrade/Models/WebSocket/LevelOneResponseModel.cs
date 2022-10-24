using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct LevelOneResponseModel
    {
        [JsonProperty(PropertyName = "key")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "1")]
        public decimal BidPrice { get; set; }

        [JsonProperty(PropertyName = "2")]
        public decimal AskPrice { get; set; }

        [JsonProperty(PropertyName = "3")]
        public decimal LastPrice { get; set; }

        [JsonProperty(PropertyName = "4")]
        public decimal BidSize { get; set; }

        [JsonProperty(PropertyName = "5")]
        public decimal AskSize { get; set; }

        [JsonProperty(PropertyName = "6")]
        public char AskID { get; set; }

        [JsonProperty(PropertyName = "7")]
        public char BidID { get; set; }

        [JsonProperty(PropertyName = "8")]
        public decimal TotalVolume { get; set; }

        [JsonProperty(PropertyName = "9")]
        public int LastSize { get; set; }

        [JsonProperty(PropertyName = "10")]
        public int TradeTime { get; set; }

        [JsonProperty(PropertyName = "16")]
        public char ExchangeID { get; set; }
    }
}
