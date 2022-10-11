using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public class ErrorModel
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; } = string.Empty;
    }
}
