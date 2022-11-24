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
    public class OrderFillMessage
    {
        public OrderFillMessageOrderGroupID OrderGroupID { get; set; }

        public DateTime ActivityTimestamp { get; set; }

        public OrderFillMessageOrder Order { get; set; }

        public string OrderCompletionCode { get; set; }

        public OrderFillMessageContraInformation ContraInformation { get; set; }

        public OrderFillMessageSettlementInformation SettlementInformation { get; set; }

        public OrderFillMessageExecutionInformation ExecutionInformation { get; set; }

        public byte MarkupAmount { get; set; }

        public byte MarkdownAmount { get; set; }

        public decimal TradeCreditAmount { get; set; }

        [XmlArrayItem("ConfirmText", IsNullable = false)]
        public object[] ConfirmTexts { get; set; }

        public byte TrueCommCost { get; set; }

        [XmlElement(DataType = "date")]
        public DateTime TradeDate { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageOrderGroupID
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
    public class OrderFillMessageOrder
    {
        public ulong OrderKey { get; set; }

        public OrderFillMessageOrderSecurity Security { get; set; }

        public OrderFillMessageOrderPricing OrderPricing { get; set; }

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

        [XmlArrayItem("Charge", IsNullable = false)]
        public OrderFillMessageOrderCharge[] Charges { get; set; }

        public ushort ClearingID { get; set; }

        public string SettlementInstructions { get; set; }

        public string EnteringDevice { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageOrderSecurity
    {
        public uint CUSIP { get; set; }

        public string Symbol { get; set; }

        public string SecurityType { get; set; }
    }

    [Serializable()]
    [XmlInclude(typeof(OrderFillMessageOrderPricingMarket))]
    [XmlInclude(typeof(OrderFillMessageOrderPricingMarketLimit))]
    [XmlInclude(typeof(OrderFillMessageOrderPricingStopMarket))]
    [XmlInclude(typeof(OrderFillMessageOrderPricingStopLimit))]
    public abstract class OrderFillMessageOrderPricing
    {
        public decimal Ask { get; set; }

        public decimal Bid { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "MarketT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderFillMessageOrderPricingMarket : OrderFillMessageOrderPricing
    { }

    [Serializable()]
    [XmlType(TypeName = "LimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderFillMessageOrderPricingMarketLimit : OrderFillMessageOrderPricing
    {
        public decimal Limit { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderFillMessageOrderPricingStopMarket : OrderFillMessageOrderPricing
    {
        public decimal Stop { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopLimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderFillMessageOrderPricingStopLimit : OrderFillMessageOrderPricing
    {
        public decimal Limit { get; set; }
        public decimal Stop { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageOrderCharge
    {
        public string Type { get; set; }

        public byte Amount { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageContraInformation
    {
        public OrderFillMessageContraInformationContra Contra { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageContraInformationContra
    {
        public uint AccountKey { get; set; }

        public string SubAccountType { get; set; }

        public string Broker { get; set; }

        public byte Quantity { get; set; }

        public object BadgeNumber { get; set; }

        public DateTime ReportTime { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageSettlementInformation
    {
        public string Instructions { get; set; }

        public string Currency { get; set; }
    }

    [Serializable()]
    public class OrderFillMessageExecutionInformation
    {
        public string Type { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal Quantity { get; set; }

        public decimal ExecutionPrice { get; set; }

        public bool AveragePriceIndicator { get; set; }

        public byte LeavesQuantity { get; set; }

        public ulong ID { get; set; }

        public object Exchange { get; set; }

        public string BrokerId { get; set; }
    }
}
