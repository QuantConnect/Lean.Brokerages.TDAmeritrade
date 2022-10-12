using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels.UserInfoAndPreferences
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
