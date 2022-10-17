using Newtonsoft.Json;
using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.TDAmeritrade.Utils.Extensions;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public class MoverModel
    {
        [JsonProperty(PropertyName = "change")]
        public decimal Change { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "direction")]
        public DirectionType Direction { get; set; }

        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "totalVolume")]
        public long TotalVolume { get; set; }
    }
}
