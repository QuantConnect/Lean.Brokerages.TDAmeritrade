using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct StreamerSubscriptionKeysModel
    {
        [JsonProperty(PropertyName = "keys")]
        public List<Keys> Keys { get; set; }
    }

    public struct Keys
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
    }
}
