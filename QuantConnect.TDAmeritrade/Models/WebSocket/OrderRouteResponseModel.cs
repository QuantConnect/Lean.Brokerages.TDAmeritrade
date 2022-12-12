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
    public class OrderRouteMessage
    {
        public OrderRouteMessageOrderGroupID OrderGroupID { get; set; }

        public DateTime ActivityTimestamp { get; set; }

        public OrderRouteMessageOrder Order { get; set; }

        public string OrderDestination { get; set; }

        public string InternalExternalRouteInd { get; set; }
    }

   [Serializable()]
    public class OrderRouteMessageOrderGroupID
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
    public class OrderRouteMessageOrder
    {
        public ulong OrderKey { get; set; }

        public OrderRouteMessageOrderSecurity Security { get; set; }

        public OrderRouteMessageOrderOrderPricing OrderPricing { get; set; }

        public string OrderType { get; set; }

        public string OrderDuration { get; set; }

        public System.DateTime OrderEnteredDateTime { get; set; }

        public string OrderInstructions { get; set; }

        public byte OriginalQuantity { get; set; }

        public string AmountIndicator { get; set; }

        public bool Discretionary { get; set; }

        public string OrderSource { get; set; }

        public bool Solicited { get; set; }

        public string MarketCode { get; set; }

        public OrderRouteMessageOrderCharges Charges { get; set; }

        public ushort ClearingID { get; set; }

        public string SettlementInstructions { get; set; }

        public string EnteringDevice { get; set; }
    }

    [Serializable()]
    public class OrderRouteMessageOrderSecurity
    {

        public uint CUSIP { get; set; }

        public string Symbol { get; set; }

        public string SecurityType { get; set; }
    }


    [Serializable()]
    [XmlInclude(typeof(OrderRouteMessageOrderOrderPricingMarket))]
    [XmlInclude(typeof(OrderRouteMessageOrderOrderPricingLimit))]
    [XmlInclude(typeof(OrderRouteMessageOrderOrderPricingStopMarket))]
    [XmlInclude(typeof(OrderRouteMessageOrderOrderPricingStopLimit))]
    public abstract class OrderRouteMessageOrderOrderPricing
    {
        public decimal Ask { get; set; }

        public decimal Bid { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "MarketT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderRouteMessageOrderOrderPricingMarket : OrderRouteMessageOrderOrderPricing
    { }

    [Serializable()]
    [XmlType(TypeName = "LimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderRouteMessageOrderOrderPricingLimit : OrderRouteMessageOrderOrderPricing
    {
        public decimal Limit { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderRouteMessageOrderOrderPricingStopMarket : OrderRouteMessageOrderOrderPricing
    {
        public decimal Stop { get; set; }
    }

    [Serializable()]
    [XmlType(TypeName = "StopLimitT", Namespace = "urn:xmlns:beb.ameritrade.com")]
    public class OrderRouteMessageOrderOrderPricingStopLimit : OrderRouteMessageOrderOrderPricing
    {
        public decimal Limit { get; set; }
        public decimal Stop { get; set; }
    }

    [Serializable()]
    public class OrderRouteMessageOrderCharges
    {
        public OrderRouteMessageOrderChargesCharge Charge { get; set; }
    }

    [Serializable()]
    public class OrderRouteMessageOrderChargesCharge
    {
        public string Type { get; set; }

        public byte Amount { get; set; }
    }
}
