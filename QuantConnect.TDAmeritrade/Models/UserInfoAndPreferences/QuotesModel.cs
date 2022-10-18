using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct QuotesModel
    {
        [JsonProperty(PropertyName = "isNyseDelayed")]
        public bool IsNyseDelayed { get; set; }

        [JsonProperty(PropertyName = "isNasdaqDelayed")]
        public bool IsNasdaqDelayed { get; set; }

        [JsonProperty(PropertyName = "isOpraDelayed")]
        public bool IsOpraDelayed { get; set; }

        [JsonProperty(PropertyName = "isAmexDelayed")]
        public bool IsAmexDelayed { get; set; }

        [JsonProperty(PropertyName = "isCmeDelayed")]
        public bool IsCmeDelayed { get; set; }

        [JsonProperty(PropertyName = "isIceDelayed")]
        public bool IsIceDelayed { get; set; }

        [JsonProperty(PropertyName = "isForexDelayed")]
        public bool IsForexDelayed { get; set; }
    }
}
