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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Orders;
using QuantConnect.Orders.TimeInForces;
using TDAmeritradeApi.Client.Models;
using TDAmeritradeApi.Client.Models.AccountsAndTrading;
using TDAmeritradeApi.Client.Models.MarketData;
using AccountsAndTrading = TDAmeritradeApi.Client.Models.AccountsAndTrading;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public static class TDAmeritradeToLeanMapper
    {
        private static readonly TDAmeritradeSymbolMapper _symbolMapper = new TDAmeritradeSymbolMapper();

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="instrument">symbol information</param>
        /// <returns>LEAN Object</returns>
        public static Symbol GetSymbolFrom(Instrument instrument)
        {
            var securityType = GetSecurityType(instrument.assetType);

            return _symbolMapper.GetLeanSymbol(instrument.symbol, securityType, Market.USA.ToString());
        }

        /// <summary>
        /// Converts a Lean symbol instance to a TD Ameritrade symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The TD Ameritrade symbol</returns>
        public static string GetBrokerageSymbol(Symbol symbol)
        {
            return _symbolMapper.GetBrokerageSymbol(symbol);
        }

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="symbol">ticker symbol</param>
        /// <param name="securityType">market where symbol belongs</param>
        /// <returns>LEAN Object</returns>
        internal static Symbol GetLeanSymbol(string symbol, SecurityType securityType)
        {
            return _symbolMapper.GetLeanSymbol(symbol, securityType, Market.USA);
        }

        /// <summary>
        /// Converts the specified TD Ameritrade order into a qc order.
        /// The 'task' will have a value if we needed to issue a rest call for the stop price, otherwise it will be null
        /// </summary>
        public static Order ConvertOrder(OrderStrategy orderStrategy)
        {
            var orderLeg = orderStrategy.orderLegCollection[0];
            var symbol = GetSymbolFrom(orderLeg.instrument);

            var quantity = orderLeg.quantity;

            var time = orderStrategy.enteredTime.Value;

            Order qcOrder;
            switch (orderStrategy.orderType)
            {
                case AccountsAndTrading.OrderType.LIMIT:
                    qcOrder = new LimitOrder(symbol, quantity, orderStrategy.price, time);
                    break;
                case AccountsAndTrading.OrderType.MARKET:
                    qcOrder = new MarketOrder(symbol, quantity, time);
                    break;
                case AccountsAndTrading.OrderType.MARKET_ON_CLOSE:
                    qcOrder = new MarketOnCloseOrder(symbol, quantity, time);
                    break;
                case AccountsAndTrading.OrderType.STOP:
                    qcOrder = new StopMarketOrder(symbol, quantity, orderStrategy.stopPrice.Value, time);
                    break;
                case AccountsAndTrading.OrderType.STOP_LIMIT:
                    qcOrder = new StopLimitOrder(symbol, quantity, orderStrategy.stopPrice.Value, orderStrategy.price, time);
                    break;
                //case AccountsAndTrading.OrderType.TRAILING_STOP:
                //    qcOrder = new TrailingStopOrder { LimitPrice = orderStrategy.price, StopPrice = orderStrategy.stopPrice.Value };
                //    break;
                //case AccountsAndTrading.OrderType.TRAILING_STOP_LIMIT:
                //    qcOrder = new TrailingStopLimitOrder { LimitPrice = orderStrategy.price, StopPrice = orderStrategy.stopPrice.Value };
                //    break;
                //case AccountsAndTrading.OrderType.NET_CREDIT:
                //case AccountsAndTrading.OrderType.NET_DEBIT:
                //case AccountsAndTrading.OrderType.NET_ZERO:
                case AccountsAndTrading.OrderType.EXERCISE:
                    qcOrder = new OptionExerciseOrder();
                    break;
                default:
                    throw new NotImplementedException($"The TD order type {orderStrategy.orderType} is not implemented.");
            }
            
            qcOrder.Status = ConvertStatus(orderStrategy.status.Value);
            qcOrder.BrokerId.Add(orderStrategy.orderId.ToStringInvariant());
            qcOrder.Properties.TimeInForce = ConvertTimeInForce(orderStrategy.duration);
            
            return qcOrder;
        }

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="duration">amount of time order is valid</param>
        /// <returns>LEAN <see cref="TimeInForce"/></returns>
        private static TimeInForce ConvertTimeInForce(OrderDurationType duration)
        {
            switch (duration)
            {
                case OrderDurationType.GOOD_TILL_CANCEL:
                    return TimeInForce.GoodTilCanceled;
                //case AccountsAndTrading.OrderDurationType.FILL_OR_KILL:
                //    break;
                default:
                    return TimeInForce.Day;
            }
        }

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="status">order status</param>
        /// <returns>LEAN <see cref="OrderStatus"/></returns>
        private static OrderStatus ConvertStatus(OrderStrategyStatusType status)
        {
            switch (status)
            {
                case OrderStrategyStatusType.QUEUED:
                    return OrderStatus.Submitted;
                case OrderStrategyStatusType.PENDING_CANCEL:
                case OrderStrategyStatusType.AWAITING_UR_OUT:
                    return OrderStatus.CancelPending;
                case OrderStrategyStatusType.WORKING:
                    return OrderStatus.PartiallyFilled;
                case OrderStrategyStatusType.REJECTED:
                    return OrderStatus.Invalid;
                case OrderStrategyStatusType.EXPIRED:
                case OrderStrategyStatusType.CANCELED:
                    return OrderStatus.Canceled;
                case OrderStrategyStatusType.PENDING_ACTIVATION:
                case OrderStrategyStatusType.AWAITING_PARENT_ORDER:
                case OrderStrategyStatusType.AWAITING_CONDITION:
                case OrderStrategyStatusType.AWAITING_MANUAL_REVIEW:
                case OrderStrategyStatusType.PENDING_REPLACE:
                    return OrderStatus.New;
                case OrderStrategyStatusType.REPLACED:
                    return OrderStatus.UpdateSubmitted;
                case OrderStrategyStatusType.ACCEPTED:
                case OrderStrategyStatusType.FILLED:
                    return OrderStatus.Filled;
                default:
                    return OrderStatus.Submitted;
            }
        }

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="assetType">the security's type</param>
        /// <returns>LEAN <see cref="SecurityType"/></returns>
        private static SecurityType GetSecurityType(InstrumentAssetType assetType)
        {
            switch (assetType)
            {
                case InstrumentAssetType.EQUITY:
                case InstrumentAssetType.ETF:
                    return SecurityType.Equity;
                case InstrumentAssetType.OPTION:
                    return SecurityType.Option;
                case InstrumentAssetType.INDEX:
                    return SecurityType.Index;
                default:
                    throw new NotSupportedException($"{assetType} is not supported.");
            }
        }

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="order">order details</param>
        /// <param name="holdingQuantity">amount you are holding</param>
        /// <returns>LEAN <see cref="OrderStrategy"/></returns>
        public static OrderStrategy ConvertToOrderStrategy(Order order, decimal holdingQuantity)
        {
            var instrumentAssetType = GetInstrumentAssetType(order.SecurityType);

            decimal? stopPrice = null;
            StopType? stopType = null;
            if (order is StopLimitOrder stopLimitOrder)
            {
                stopPrice = stopLimitOrder.StopPrice;
                stopType = StopType.MARK;
            }
            else if (order is StopMarketOrder stopMarketOrder)
            {
                stopPrice = stopMarketOrder.StopPrice;
                stopType = StopType.MARK;
            }

            return new OrderStrategy()
            {
                complexOrderStrategyType = ComplexOrderStrategyType.NONE, //Do not have brokerage create spread have QC do it.
                orderType = GetOrderType(order.Type),
                session = OrderStrategySessionType.NORMAL,
                price = order.Price,
                stopPrice = stopPrice,
                stopType = stopType,
                duration = GetDuration(order.TimeInForce),
                orderStrategyType = GetStrategyType(order),
                orderLegCollection = new OrderLeg[]
                {
                    new OrderLeg()
                    {
                        orderLegType = instrumentAssetType,
                        instruction = GetOrderInstruction(instrumentAssetType, order, holdingQuantity),
                        quantity = order.Quantity,
                        instrument = new Instrument()
                        {
                            symbol = _symbolMapper.GetBrokerageSymbol(order.Symbol),
                            assetType = instrumentAssetType
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Convert TD Ameritrade API object to LEAN Object
        /// </summary>
        /// <param name="instrumentAssetType">the security's type</param>
        /// <returns>LEAN <see cref="OrderInstructionType"/></returns>
        private static OrderInstructionType GetOrderInstruction(InstrumentAssetType instrumentAssetType, Order order, decimal holdingQuantity)
        {
            if (instrumentAssetType == InstrumentAssetType.OPTION)
            {
                if (order.Direction == OrderDirection.Buy)
                {
                    if (holdingQuantity >= 0)
                        return OrderInstructionType.BUY_TO_OPEN;
                    else
                        return OrderInstructionType.BUY_TO_CLOSE;
                }
                else
                {
                    if (holdingQuantity > 0)
                        return OrderInstructionType.SELL_TO_CLOSE;
                    else
                        return OrderInstructionType.SELL_TO_OPEN;
                }
            }
            else
            {

                if (order.Direction == OrderDirection.Buy)
                {
                    if (holdingQuantity >= 0)
                        return OrderInstructionType.BUY;
                    else
                        return OrderInstructionType.BUY_TO_COVER;
                }
                else
                {
                    if (holdingQuantity > 0)
                        return OrderInstructionType.SELL;
                    else
                        return OrderInstructionType.SELL_SHORT;
                }
            }
        }

        /// <summary>
        /// Convert LEAN Object to TD Ameritrade API object
        /// </summary>
        /// <param name="securityType">the security's type</param>
        /// <returns>TD Ameritrade API <see cref="InstrumentAssetType"/></returns>
        private static InstrumentAssetType GetInstrumentAssetType(SecurityType securityType)
        {
            switch (securityType)
            {
                case SecurityType.Equity:
                    return InstrumentAssetType.EQUITY;
                case SecurityType.Option:
                case SecurityType.IndexOption:
                    return InstrumentAssetType.OPTION;
                case SecurityType.Index:
                    return InstrumentAssetType.INDEX;
                default:
                    throw new NotSupportedException($"{securityType} is not supported.");
            }
        }

        /// <summary>
        /// Convert LEAN Object to TD Ameritrade API object
        /// </summary>
        /// <param name="order">order details</param>
        /// <returns>TD Ameritrade API <see cref="OrderStrategyType"/></returns>
        private static OrderStrategyType GetStrategyType(Order order)
        {
            if (order is StopLimitOrder stopLimitOrder)
            {
                if (stopLimitOrder.LimitPrice != 0 && stopLimitOrder.StopPrice != 0)
                    return OrderStrategyType.OCO;
            }

            return OrderStrategyType.SINGLE;
        }

        /// <summary>
        /// Convert LEAN Object to TD Ameritrade API object
        /// </summary>
        /// <param name="timeInForce">length of time order is good for</param>
        /// <returns>TD Ameritrade API <see cref="OrderDurationType"/></returns>
        private static OrderDurationType GetDuration(TimeInForce timeInForce)
        {
            if (timeInForce is DayTimeInForce)
                return OrderDurationType.DAY;
            else
                return OrderDurationType.GOOD_TILL_CANCEL;
        }

        /// <summary>
        /// Convert LEAN Object to TD Ameritrade API object
        /// </summary>
        /// <param name="type">order type</param>
        /// <returns>TD Ameritrade API <see cref="AccountsAndTrading.OrderType"/></returns>
        private static AccountsAndTrading.OrderType GetOrderType(Orders.OrderType type)
        {
            switch (type)
            {
                case Orders.OrderType.MarketOnOpen:
                case Orders.OrderType.Market:
                    return AccountsAndTrading.OrderType.MARKET;
                case Orders.OrderType.Limit:
                case Orders.OrderType.LimitIfTouched:
                    return AccountsAndTrading.OrderType.LIMIT;
                case Orders.OrderType.StopMarket:
                    return AccountsAndTrading.OrderType.STOP;
                case Orders.OrderType.StopLimit:
                    return AccountsAndTrading.OrderType.STOP_LIMIT;
                case Orders.OrderType.MarketOnClose:
                    return AccountsAndTrading.OrderType.MARKET_ON_CLOSE;
                case Orders.OrderType.OptionExercise:
                    return AccountsAndTrading.OrderType.EXERCISE;
                default:
                    throw new NotSupportedException($"{type} is not supported.");
            }
        }
    }
}
