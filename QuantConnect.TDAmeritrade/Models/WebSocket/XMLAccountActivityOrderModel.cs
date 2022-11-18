﻿using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn:xmlns:beb.ameritrade.com")]
    [XmlRoot(Namespace = "urn:xmlns:beb.ameritrade.com", IsNullable = false)]
    public class OrderEntryRequestMessage
    {
        public OrderGroupID OrderGroupID { get; set; }

        public DateTime ActivityTimestamp { get; set; }

        public OrderEntryRequestMessageOrder Order { get; set; }

        public DateTime LastUpdated { get; set; }

        [XmlArrayItem("ConfirmText", IsNullable = false)]
        public object[] ConfirmTexts { get; set; }
    }

    [Serializable()]
    public class OrderGroupID
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
    public class OrderEntryRequestMessageOrder
    {
        public ulong OrderKey { get; set; } 

        public OrderEntryRequestMessageOrderSecurity Security { get; set;  }

        public OrderEntryRequestMessageOrderOrderPricing OrderPricing { get; set; }

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

        public OrderEntryRequestMessageOrderCharges Charges { get; set; }

        public ushort ClearingID { get; set; }

        public string SettlementInstructions { get; set; }
        public string EnteringDevice { get; set; }
    }

    [Serializable()]
    public class OrderEntryRequestMessageOrderSecurity
    {
        public string CUSIP { get; set; }

        public string Symbol { get; set; }
        public string SecurityType { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "LimitT")]
    public class OrderEntryRequestMessageOrderOrderPricing
    {
        public decimal Ask { get; set; }

        public decimal Bid { get; set; }

        public decimal Limit { get; set; }
    }

    [Serializable()]
    public class OrderEntryRequestMessageOrderCharges
    {
        public OrderEntryRequestMessageOrderChargesCharge Charge { get; set; }
    }

    [Serializable()]
    public class OrderEntryRequestMessageOrderChargesCharge
    {
        public string Type { get; set; }

        public byte Amount { get; set; }
    }
}
