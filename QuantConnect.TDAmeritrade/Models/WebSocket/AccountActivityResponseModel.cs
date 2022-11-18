using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct AccountActivityResponseModel
    {
        [JsonProperty(PropertyName = "1")]
        public string AccountNumber { get; set; }

        [JsonProperty(PropertyName = "2")]
        public string MessageType { get; set; }

        [JsonProperty(PropertyName = "3")]
        public string MessageData { get; set; }
    }
}
