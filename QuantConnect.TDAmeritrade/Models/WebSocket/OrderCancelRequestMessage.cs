using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn:xmlns:beb.ameritrade.com")]
    [XmlRoot(Namespace = "urn:xmlns:beb.ameritrade.com", IsNullable = false)]
    public class OrderCancelRequestMessage
    {
        public OrderCancelRequestMessageOrderGroupID OrderGroupID { get; set; }

        public DateTime ActivityTimestamp { get; set; }

        public OrderCancelRequestMessageOrder Order { get; set; }

        public DateTime LastUpdated { get; set; } 

        [XmlArrayItem("ConfirmText", IsNullable = false)]
        public object[] ConfirmTexts { get; set; }

        public byte PendingCancelQuantity { get; set; }
    }

    [Serializable()]
    public class OrderCancelRequestMessageOrderGroupID
    {
        public byte Firm { get; set; }

        public byte Branch { get; set; }

        public uint ClientKey { get; set; }

        public uint AccountKey { get; set; }

        public string Segment { get; set; }

        public string SubAccountType { get; set; }

        public string CDDomainID { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "EquityOrderT")]
    public class OrderCancelRequestMessageOrder
    {
        public ulong OrderKey { get; set; }

        public OrderCancelRequestMessageOrderSecurity Security { get; set; }

        public OrderCancelRequestMessageOrderOrderPricing OrderPricing { get; set; }

        public string OrderType { get; set; }

        public string OrderDuration { get; set; }

        public DateTime OrderEnteredDateTime { get; set; }

        public string OrderInstructions { get; set; }

        public byte OriginalQuantity { get; set; }

        public string AmountIndicator { get; set; }

        public bool Discretionary { get; set; }

        public string OrderSource { get; set; }

        public bool Solicited { get; set; }

        public string MarketCode { get; set; }

        public OrderCancelRequestMessageOrderCharges Charges { get; set; }

        public ushort ClearingID { get; set; }

        public string SettlementInstructions { get; set; }

        public string EnteringDevice { get; set; }
    }

    [Serializable()]
    public class OrderCancelRequestMessageOrderSecurity
    {
        public uint CUSIP { get; set; }

        public string Symbol { get; set; }

        public string SecurityType { get; set; }
    }

    [Serializable()]
    [XmlInclude(typeof(OrderCancelRequestMessageOrderOrderPricingLimit))]
    [XmlInclude(typeof(OrderCancelRequestMessageOrderOrderPricingMarket))]
    [XmlInclude(typeof(OrderCancelRequestMessageOrderOrderPricingStopMarket))]
    [XmlInclude(typeof(OrderCancelRequestMessageOrderOrderPricingStopLimit))]

    public abstract class OrderCancelRequestMessageOrderOrderPricing
    {
        public decimal Ask { get; set; }

        public decimal Bid { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "LimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderCancelRequestMessageOrderOrderPricingLimit : OrderCancelRequestMessageOrderOrderPricing
    {
        public decimal Limit { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "MarketT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderCancelRequestMessageOrderOrderPricingMarket : OrderCancelRequestMessageOrderOrderPricing
    { }

    [Serializable()]
    [XmlType(TypeName = "StopT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderCancelRequestMessageOrderOrderPricingStopMarket : OrderCancelRequestMessageOrderOrderPricing
    {
        public decimal Stop { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopLimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderCancelRequestMessageOrderOrderPricingStopLimit : OrderCancelRequestMessageOrderOrderPricing
    {
        public decimal Limit { get; set; }
        public decimal Stop { get; set; }
    }

    [Serializable()]
    public class OrderCancelRequestMessageOrderCharges
    {
        public OrderCancelRequestMessageOrderChargesCharge Charge { get; set; }
    }

    [Serializable()]
    public class OrderCancelRequestMessageOrderChargesCharge
    {
        public string Type { get; set; }

        public byte Amount { get; set; }
    }
}
