using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class InstrumentFundamentalModel
    {
        [JsonProperty(PropertyName = "fundamental")]
        public InstrumentFundamentalDataModel? Fundamental { get; set; }

        [JsonProperty(PropertyName = "cusip")]
        public string Cusip { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "exchange")]
        public string Exchange { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "assetType")]
        public string AssetType { get; set; } = string.Empty;
    }
}
