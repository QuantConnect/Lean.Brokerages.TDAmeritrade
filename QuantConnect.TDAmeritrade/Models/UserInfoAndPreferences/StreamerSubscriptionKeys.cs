using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class StreamerSubscriptionKeys
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
    }
}
