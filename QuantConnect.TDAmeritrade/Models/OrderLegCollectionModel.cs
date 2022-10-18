using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    [JsonObject(Title = "orderLegCollection")]
    public class OrderLegCollectionModel : PlaceOrderLegCollectionModel
    {
        [JsonProperty(PropertyName = "orderLegType")]
        public string OrderLegType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "legId")]
        public ulong LegId { get; set; }

        [JsonProperty(PropertyName = "positionEffect")]
        public string PositionEffect { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "instrument")]
        public new InstrumentModel Instrument { get; set; } = new();
    }
}
