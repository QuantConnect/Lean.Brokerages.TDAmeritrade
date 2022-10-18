using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class MarketHoursModel
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "exchange")]
        public string Exchange { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "isOpen")]
        public bool IsOpen { get; set; }

        [JsonProperty(PropertyName = "marketType")]
        public string MarketType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sessionHours")]
        public Dictionary<string, StartEndMarketTime[]> SessionHours { get; set; } = new();
    }

    public struct StartEndMarketTime
    {
        [JsonProperty(PropertyName = "start")]
        public string Start { get; set; }

        [JsonProperty(PropertyName = "end")]
        public string End { get; set; }
    }


}
