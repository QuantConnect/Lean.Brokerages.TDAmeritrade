using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    [Serializable]
    public class StreamRequestModel
    {
        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }
        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }
        [JsonProperty(PropertyName = "requestid")]
        public int Requestid { get; set; }
        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
        [JsonProperty(PropertyName = "parameters")]
        public object Parameters { get; set; }
    }

    [Serializable]
    public class StreamRequestModelContainer
    {
        [JsonProperty(PropertyName = "requests")]
        public StreamRequestModel[] Requests { get; set; }
    }
}
