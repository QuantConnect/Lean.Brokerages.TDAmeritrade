using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class ErrorModel
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; } = string.Empty;
    }
}
