using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class OrderModel
    {
        [JsonProperty(PropertyName = "session")]
        public string Session { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "orderType")]
        public string OrderType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "complexOrderStrategyType")]
        public string ComplexOrderStrategyType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty(PropertyName = "filledQuantity")]
        public decimal FilledQuantity { get; set; }

        [JsonProperty(PropertyName = "remainingQuantity")]
        public decimal RemainingQuantity { get; set; }

        [JsonProperty(PropertyName = "requestedDestination")]
        public string RequestedDestination { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "destinationLinkName")]
        public string DestinationLinkName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "stopPrice")]
        public decimal StopPrice { get; set; }

        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }

        [JsonProperty(PropertyName = "orderLegCollection")]
        public List<OrderLegCollectionModel> OrderLegCollections { get; set; } = new();

        [JsonProperty(PropertyName = "orderStrategyType")]
        public string OrderStrategyType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "orderId")]
        public ulong OrderId { get; set; }

        [JsonProperty(PropertyName = "cancelable")]
        public bool Cancelable { get; set; }

        [JsonProperty(PropertyName = "editable")]
        public bool Editable { get; set; }

        [JsonProperty(PropertyName = "status")]
        public OrderStatusType Status { get; set; }

        [JsonProperty(PropertyName = "enteredTime")]
        public string EnteredTime { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "closeTime")]
        public string CloseTime { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "accountId")]
        public ulong AccountId { get; set; }

        [JsonProperty(PropertyName = "statusDescription")]
        public string StatusDescription { get; set; } = string.Empty;
    }
}
