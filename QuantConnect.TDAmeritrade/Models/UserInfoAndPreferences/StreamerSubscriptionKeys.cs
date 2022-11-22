using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct StreamerSubscriptionKeys
    {
        [JsonProperty(PropertyName = "keys")]
        public Keys[] Keys { get; set; }
    }

    public struct Keys
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
    }
}
