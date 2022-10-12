using Newtonsoft.Json;
using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.TDAmeritrade.Utils.Extensions;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    [JsonObject(Title = "orderLegCollection")]
    public class PlaceOrderLegCollectionModel
    {
        [JsonProperty(PropertyName = "quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty(PropertyName = "instrument")]
        public InstrumentPlaceOrderModel Instrument { get; set; } = new();

        [JsonProperty(PropertyName = "instruction")]
        public string InstructionType { get; set; } = string.Empty;

        public PlaceOrderLegCollectionModel()
        { }

        public PlaceOrderLegCollectionModel(InstructionType instructionType, decimal quantity, InstrumentPlaceOrderModel instrument)
        {
            InstructionType = instructionType.GetEnumMemberValue();
            Quantity = quantity;
            Instrument = instrument;
        }
    }
}
