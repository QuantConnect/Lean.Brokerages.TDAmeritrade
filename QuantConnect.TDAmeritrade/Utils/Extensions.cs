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

using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Orders;
using System.Reflection;
using System.Runtime.Serialization;
using OrderTypeBrokerage = QuantConnect.Brokerages.TDAmeritrade.Models.OrderType;

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

        public static string ConvertInstructionTypeToString(this InstructionType instructionType) => instructionType switch
        {
            InstructionType.Buy => "BUY",
            InstructionType.Sell => "SELL",
            InstructionType.BuyToCover => "BUY_TO_COVER",
            InstructionType.BuyToOpen => "BUY_TO_OPEN",
            InstructionType.BuyToClose => "BUY_TO_CLOSE",
            InstructionType.SellToOpen => "SELL_TO_OPEN",
            InstructionType.SellToClose => "SELL_TO_CLOSE",
            InstructionType.Exchange => "EXCHANGE",
            _ => throw new ArgumentOutOfRangeException(nameof(instructionType), $"Not expected InstructionType value: {instructionType}")
        };

        public static InstructionType ConvertStringToInstructionType(this string instructionType) => instructionType switch
        {
            "BUY" => InstructionType.Buy,
            "SELL" => InstructionType.Sell,
            "BUY_TO_COVER" => InstructionType.BuyToCover,
            "BUY_TO_OPEN" => InstructionType.BuyToOpen,
            "BUY_TO_CLOSE" => InstructionType.BuyToClose,
            "SELL_TO_OPEN" => InstructionType.SellToOpen,
            "SELL_TO_CLOSE" => InstructionType.SellToClose,
            "EXCHANGE" => InstructionType.Exchange,
            _ => throw new ArgumentOutOfRangeException(nameof(instructionType), $"Not expected InstructionType value: {instructionType}")
        };

        public static string ConvertPeriodTypeToString(this PeriodType periodType) => periodType switch
        {
            PeriodType.Day => "day",
            PeriodType.Month => "month",
            PeriodType.Year => "year",
            PeriodType.Ytd => "ytd",
            _ => throw new ArgumentOutOfRangeException(nameof(periodType), $"Not expected PeriodType value: {periodType}")
        };

        public static string ConvertFrequencyTypeToString(this FrequencyType frequencyType) => frequencyType switch
        {
            FrequencyType.NoValue => "novalue",
            FrequencyType.Minute => "minute",
            FrequencyType.Daily => "daily",
            FrequencyType.Weekly => "weekly",
            FrequencyType.Monthly => "monthly",
            _ => throw new ArgumentOutOfRangeException(nameof(frequencyType), $"Not expected FrequencyType value: {frequencyType}")
        };

        public static string ConvertGrantTypeToString(this GrantType grantType) => grantType switch
        {
            GrantType.AuthorizationCode => "authorization_code",
            GrantType.RefreshToken => "refresh_token",
            _ => throw new ArgumentOutOfRangeException(nameof(grantType), $"Not expected GrantType value: {grantType}")
        };

        public static string ConvertOrderTypeToString(this OrderTypeBrokerage orderType) => orderType switch
        {
            OrderTypeBrokerage.Market => "MARKET",
            OrderTypeBrokerage.Limit => "LIMIT",
            OrderTypeBrokerage.Stop => "STOP",
            OrderTypeBrokerage.StopLimit => "STOP_LIMIT",
            OrderTypeBrokerage.TrailingStop => "TRAILING_STOP",
            OrderTypeBrokerage.MarketOnClose => "MARKET_ON_CLOSE",
            OrderTypeBrokerage.Exercise => "EXERCISE",
            OrderTypeBrokerage.TrailingStopLimit => "TRAILING_STOP_LIMIT",
            OrderTypeBrokerage.NetDebit => "NET_DEBIT",
            OrderTypeBrokerage.NetCredit => "NET_CREDIT",
            OrderTypeBrokerage.NetZero => "NET_ZERO",
            _ => throw new ArgumentOutOfRangeException(nameof(orderType), $"Not expected OrderTypeBrokerage value: {orderType}")
        };

        public static OrderTypeBrokerage ConvertStringToOrderTypeBrokerage(this string orderType) => orderType switch
        {
            "MARKET" => OrderTypeBrokerage.Market,
            "LIMIT" => OrderTypeBrokerage.Limit,
            "STOP" => OrderTypeBrokerage.Stop,
            "STOP_LIMIT" => OrderTypeBrokerage.StopLimit,
            "TRAILING_STOP" => OrderTypeBrokerage.TrailingStop,
            "MARKET_ON_CLOSE" => OrderTypeBrokerage.MarketOnClose,
            "EXERCISE" => OrderTypeBrokerage.Exercise,
            "TRAILING_STOP_LIMIT" => OrderTypeBrokerage.TrailingStopLimit,
            "NET_DEBIT" => OrderTypeBrokerage.NetDebit,
            "NET_CREDIT" => OrderTypeBrokerage.NetCredit,
            "NET_ZERO" => OrderTypeBrokerage.NetZero,
            _ => throw new ArgumentOutOfRangeException(nameof(orderType), $"Not expected OrderTypeBrokerage value: {orderType}")
        };

        public static string ConvertSessionTypeToString(this SessionType sessionType) => sessionType switch
        {
            SessionType.Normal => "NORMAL",
            SessionType.AM => "AM",
            SessionType.PM => "PM",
            SessionType.Seamless => "SEAMLESS",
            _ => throw new ArgumentOutOfRangeException(nameof(sessionType), $"Not expected SessionType value: {sessionType}")
        };

        public static string ConvertDurationTypeToString(this DurationType durationType) => durationType switch
        {
            DurationType.Day => "DAY",
            DurationType.GoodTillCancel => "GOOD_TILL_CANCEL",
            DurationType.FullOrKill => "FILL_OR_KILL",
            _ => throw new ArgumentOutOfRangeException(nameof(durationType), $"Not expected DurationType value: {durationType}")
        };

        public static string ConvertOrderStrategyTypeToString(this OrderStrategyType orderStrategyType) => orderStrategyType switch
        {
            OrderStrategyType.Single => "SINGLE",
            OrderStrategyType.Oco => "OCO",
            OrderStrategyType.Trigger => "TRIGGER",
            _ => throw new ArgumentOutOfRangeException(nameof(orderStrategyType), $"Not expected OrderStrategyType value: {orderStrategyType}")
        };

        public static string ConvertComplexOrderStrategyTypeToString(this ComplexOrderStrategyType complexOrderStrategyType) => complexOrderStrategyType switch
        {
            ComplexOrderStrategyType.None => "NONE",
            ComplexOrderStrategyType.Covered => "COVERED",
            ComplexOrderStrategyType.Vertical => "VERTICAL",
            ComplexOrderStrategyType.BackRatio => "BACK_RATIO",
            ComplexOrderStrategyType.Calendar => "CALENDAR",
            ComplexOrderStrategyType.Diagonal => "DIAGONAL",
            ComplexOrderStrategyType.Straddle => "STRADDLE",
            ComplexOrderStrategyType.Strangle => "STRANGLE",
            ComplexOrderStrategyType.CollarSynthetic => "COLLAR_SYNTHETIC",
            ComplexOrderStrategyType.Butterfly => "BUTTERFLY",
            ComplexOrderStrategyType.Condor => "CONDOR",
            ComplexOrderStrategyType.IronCondor => "IRON_CONDOR",
            ComplexOrderStrategyType.VerticalRoll => "VERTICAL_ROLL",
            ComplexOrderStrategyType.CollarWithStock => "COLLAR_WITH_STOCK",
            ComplexOrderStrategyType.DoubleDiagonal => "DOUBLE_DIAGONAL",
            ComplexOrderStrategyType.UnbalancedButterfly => "UNBALANCED_BUTTERFLY",
            ComplexOrderStrategyType.UnbalancedCondor => "UNBALANCED_CONDOR",
            ComplexOrderStrategyType.UnbalancedIronCondor => "UNBALANCED_IRON_CONDOR",
            ComplexOrderStrategyType.UnbalancedVerticalRoll => "UNBALANCED_VERTICAL_ROLL",
            ComplexOrderStrategyType.Custom => "CUSTOM",
            _ => throw new ArgumentOutOfRangeException(nameof(complexOrderStrategyType), $"Not expected ComplexOrderStrategyType value: {complexOrderStrategyType}")
        };

        public static string ConvertOrderStatusTypeToString(this OrderStatusType orderStatusType) => orderStatusType switch
        {
            OrderStatusType.NoValue => "No Value",
            OrderStatusType.Awaiting_Parent_Order => "AWAITING_PARENT_ORDER",
            OrderStatusType.Awaiting_Condition => "AWAITING_CONDITION",
            OrderStatusType.Awaiting_Manual_Review => "AWAITING_MANUAL_REVIEW",
            OrderStatusType.Accepted => "ACCEPTED",
            OrderStatusType.Awaiting_Ur_Out => "AWAITING_UR_OUT",
            OrderStatusType.Pending_Activation => "PENDING_ACTIVATION",
            OrderStatusType.Queued => "QUEUED",
            OrderStatusType.Working => "WORKING",
            OrderStatusType.Rejected => "REJECTED",
            OrderStatusType.Pending_Cancel => "PENDING_CANCEL",
            OrderStatusType.Canceled => "CANCELED",
            OrderStatusType.Pending_Replace => "PENDING_REPLACE",
            OrderStatusType.Replaced => "REPLACED",
            OrderStatusType.Filled => "FILLED",
            OrderStatusType.Expired => "EXPIRED",
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatusType), $"Not expected OrderStatusType value: {orderStatusType}")
        };

        public static int ResolutionToFrequency(this Resolution resolution) => resolution switch
        {
            Resolution.Minute => 1,
            Resolution.Hour => 60,
            Resolution.Daily => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), $"Not expected Resolution value: {resolution}")
        };

        public static Order ConvertOrder(this OrderModel order)
        {
            Order qcOrder;

            var symbol = order.OrderLegCollections[0].Instrument.Symbol;// _symbolMapper.GetLeanSymbol(order.Class == TradierOrderClass.Option ? order.OptionSymbol : order.Symbol);
            var quantity = ConvertQuantity(order.Quantity, order.OrderLegCollections[0].InstructionType.ConvertStringToInstructionType());
            var time = order.EnteredTime;

            switch (order.OrderType.ConvertStringToOrderTypeBrokerage())
            {
                case OrderTypeBrokerage.Market:
                    qcOrder = new MarketOrder(symbol, quantity, time);
                    break;
                case OrderTypeBrokerage.Limit:
                    qcOrder = new LimitOrder(symbol, quantity, order.Price, time);
                    break;
                case OrderTypeBrokerage.Stop:
                    qcOrder = new StopMarketOrder(symbol, quantity, order.StopPrice, time);
                    break;
                case OrderTypeBrokerage.StopLimit:
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

                case OrderStatusType.Pending_Activation:
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

        public static OrderTypeBrokerage ConvertLeanOrderTypeToExchange(this Orders.OrderType orderType) => orderType switch
        {
            Orders.OrderType.Market => OrderTypeBrokerage.Market,
            Orders.OrderType.Limit => OrderTypeBrokerage.Limit,
            Orders.OrderType.StopLimit => OrderTypeBrokerage.StopLimit,
            Orders.OrderType.StopMarket => OrderTypeBrokerage.Stop,
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
