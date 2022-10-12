using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public class InstrumentPlaceOrderModel
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "assetType")]
        public string AssetType { get; set; } = string.Empty;

        public InstrumentPlaceOrderModel()
        { }

        public InstrumentPlaceOrderModel(string symbol, string assetType)
        {
            Symbol = symbol;
            AssetType = assetType;
        }
    }
}
