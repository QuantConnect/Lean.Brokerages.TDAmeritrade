/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Xml.Serialization;

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

        public SerializableSecurity Security { get; set; }

        public OrderEntryRequestMessageOrderOrderPricing OrderPricing { get; set; }

        public OrderTypeWebSocket OrderType { get; set; }

        public string OrderDuration { get; set; }

        public DateTime OrderEnteredDateTime { get; set; }

        public OrderInstructionsWebSocket OrderInstructions { get; set; }

        public decimal OriginalQuantity { get; set; }

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
    [XmlInclude(typeof(OrderEntryRequestMessageOrderOrderPricingLimit))]
    [XmlInclude(typeof(OrderEntryRequestMessageOrderOrderPricingMarket))]
    [XmlInclude(typeof(OrderEntryRequestMessageOrderOrderPricingStopMarket))]
    [XmlInclude(typeof(OrderEntryRequestMessageOrderOrderPricingStopLimit))]
    public abstract class OrderEntryRequestMessageOrderOrderPricing
    {
        public decimal Ask { get; set; }

        public decimal Bid { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "LimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderEntryRequestMessageOrderOrderPricingLimit : OrderEntryRequestMessageOrderOrderPricing
    {
        public decimal Limit { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "MarketT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderEntryRequestMessageOrderOrderPricingMarket : OrderEntryRequestMessageOrderOrderPricing
    { }

    [Serializable()]
    [XmlType(TypeName = "StopT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderEntryRequestMessageOrderOrderPricingStopMarket : OrderEntryRequestMessageOrderOrderPricing
    {
        public decimal Stop { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopLimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderEntryRequestMessageOrderOrderPricingStopLimit : OrderEntryRequestMessageOrderOrderPricing
    {
        public decimal Limit { get; set; }
        public decimal Stop { get; set; }
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
