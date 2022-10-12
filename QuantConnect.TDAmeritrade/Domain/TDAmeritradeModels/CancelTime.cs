using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public struct CancelTime
    {
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "shortFormat")]
        public bool ShortFormat { get; set; }
    }
}
