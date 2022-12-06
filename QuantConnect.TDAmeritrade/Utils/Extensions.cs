﻿/*
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

using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Orders;
using System.Reflection;
using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Utils
{
    public static class Extensions
    {
        public static string GetProjectionTypeInRequestFormat(this ProjectionType projectType) => projectType switch
        {
            ProjectionType.SymbolSearch => "symbol-search",
            ProjectionType.Fundamental => "fundamental",
            _ => throw new ArgumentOutOfRangeException(nameof(projectType), $"Not expected direction value: {projectType}")
        };

        public static string? GetEnumMemberValue<T>(this T value)
            where T : Enum
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }

        public static int ResolutionToFrequency(this Resolution resolution) => resolution switch
        {
            Resolution.Minute => 1,
            Resolution.Hour => 60,
            Resolution.Daily => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), $"Not expected Resolution value: {resolution}")
        };

        public static string GetEnumValue(this Enum value)
        {
            // Get the Description attribute value for the enum value
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (EnumMemberAttribute[])fi.GetCustomAttributes(typeof(EnumMemberAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Value;
            }
            else
            {
                return value.ToString();
            }
        }

        public static T ToEnum<T>(this string str)
        {
            var enumType = typeof(T);
            foreach (var name in Enum.GetNames(enumType))
            {
                var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
                if (enumMemberAttribute.Value == str) return (T)Enum.Parse(enumType, name);
            }
            //throw exception or whatever handling you want or
            return default;
        }

        public static Order ConvertOrder(this OrderModel order)
        {
            Order qcOrder;

            var symbol = order.OrderLegCollections[0].Instrument.Symbol;// _symbolMapper.GetLeanSymbol(order.Class == TradierOrderClass.Option ? order.OptionSymbol : order.Symbol);
            var quantity = ConvertQuantity(order.Quantity, order.OrderLegCollections[0].InstructionType.ToEnum<InstructionType>());
            var time = order.EnteredTime;

            switch (order.OrderType.ToEnum<Models.OrderType>())
            {
                case Models.OrderType.Market:
                    qcOrder = new MarketOrder(symbol, quantity, time);
                    break;
                case Models.OrderType.Limit:
                    qcOrder = new LimitOrder(symbol, quantity, order.Price, time);
                    break;
                case Models.OrderType.Stop:
                    qcOrder = new StopMarketOrder(symbol, quantity, order.StopPrice, time);
                    break;
                case Models.OrderType.StopLimit:
                    qcOrder = new StopLimitOrder(symbol, quantity, order.StopPrice, order.Price, time);
                    break;
                default:
                    throw new NotImplementedException("The Tradier order type " + order.OrderType + " is not implemented.");
            }

            qcOrder.Status = ConvertStatus(order.Status);
            qcOrder.BrokerId.Add(order.OrderId.ToStringInvariant());
            return qcOrder;
        }

        private static int ConvertQuantity(decimal quantity, InstructionType instructionType)
        {
            switch (instructionType)
            {
                case InstructionType.Buy:
                case InstructionType.BuyToCover:
                case InstructionType.BuyToClose:
                case InstructionType.BuyToOpen:
                    return (int)quantity;

                case InstructionType.SellToClose:
                case InstructionType.Sell:
                case InstructionType.SellToOpen:
                    return -(int)quantity;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static OrderStatus ConvertStatus(OrderStatusType status)
        {
            switch (status)
            {
                case OrderStatusType.Filled:
                    return OrderStatus.Filled;

                case OrderStatusType.Canceled:
                    return OrderStatus.Canceled;

                case OrderStatusType.PendingActivation:
                    return OrderStatus.Submitted;

                case OrderStatusType.Expired:
                case OrderStatusType.Rejected:
                    return OrderStatus.Invalid;

                case OrderStatusType.Queued:
                    return OrderStatus.New;

                case OrderStatusType.Working:
                    return OrderStatus.PartiallyFilled;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Models.OrderType ConvertLeanOrderTypeToExchange(this Orders.OrderType orderType) => orderType switch
        {
            Orders.OrderType.Market => Models.OrderType.Market,
            Orders.OrderType.Limit => Models.OrderType.Limit,
            Orders.OrderType.StopLimit => Models.OrderType.StopLimit,
            Orders.OrderType.StopMarket => Models.OrderType.Stop,
            _ => throw new ArgumentException($"TDAmeritrade doesn't support of OrderType {nameof(orderType)}")
        };

        public static InstructionType ConvertLeanOrderDirectionToExchange(this OrderDirection orderDirection) => orderDirection switch
        {
            OrderDirection.Buy => InstructionType.Buy,
            OrderDirection.Sell => InstructionType.Sell,
            _ => throw new ArgumentException($"TDAmeritrade doesn't support of OrderDirection {nameof(orderDirection)}")
        };

        /// <summary>
        /// Orders above $1 can be entered in no more than 2 decimals; orders below $1 can be entered in no more than 4 decimals
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static decimal RoundAmountToExachangeFormat(this decimal amount)
            => amount < 1m ? amount.RoundToSignificantDigits(4) : amount.RoundToSignificantDigits(2);
    }
}