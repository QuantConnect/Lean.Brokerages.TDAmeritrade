using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn:xmlns:beb.ameritrade.com")]
    [XmlRoot(Namespace = "urn:xmlns:beb.ameritrade.com", IsNullable = false)]
    public class UROUTMessage
    {
        public UROUTMessageOrderGroupID OrderGroupID { get; set; }

        public DateTime ActivityTimestamp { get; set; }

        public UROUTMessageOrder Order { get; set; }

        public string OrderDestination { get; set; }

        public string InternalExternalRouteInd { get; set; }

        public byte CancelledQuantity { get; set; }
    }

    [Serializable()]
    public class UROUTMessageOrderGroupID
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
    public class UROUTMessageOrder
    {
        public ulong OrderKey { get; set; }

        public UROUTMessageOrderSecurity Security { get; set; }

        public UROUTMessageOrderPricing OrderPricing { get; set; }

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

        public ushort ClearingID { get; set; }

        public string SettlementInstructions { get; set; }

        public string EnteringDevice { get; set; }
    }

    [Serializable()]
    public class UROUTMessageOrderSecurity
    {
        public uint CUSIP { get; set; }

        public string Symbol { get; set; }

        public string SecurityType { get; set; }
    }

    [Serializable()]
    [XmlInclude(typeof(UROUTMessageOrderPricingLimit))]
    [XmlInclude(typeof(UROUTMessageOrderPricingMarket))]
    [XmlInclude(typeof(UROUTMessageOrderPricingStopMarket))]
    [XmlInclude(typeof(UROUTMessageOrderPricingStopLimit))]
    public abstract class UROUTMessageOrderPricing
    {
        public decimal Ask { get; set; }

        public decimal Bid { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "LimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class UROUTMessageOrderPricingLimit : UROUTMessageOrderPricing
    {
        public decimal Limit { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "MarketT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class UROUTMessageOrderPricingMarket : UROUTMessageOrderPricing
    { }

    [Serializable()]
    [XmlType(TypeName = "StopT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class UROUTMessageOrderPricingStopMarket : UROUTMessageOrderPricing
    {
        public decimal Stop { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopLimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class UROUTMessageOrderPricingStopLimit : UROUTMessageOrderPricing
    {
        public decimal Limit { get; set; }
        public decimal Stop { get; set; }
    }
}
