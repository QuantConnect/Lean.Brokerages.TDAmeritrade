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

using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum OrderType
    {
        /// <summary>
        /// An order seeking execution of a buy or sell transaction immediately at the next available market price. 
        /// </summary>
        [EnumMember(Value = "MARKET")]
        Market = 0,
        /// <summary>
        /// An order seeking execution of a buy or sell transaction at a specified price or better.
        /// </summary>
        [EnumMember(Value = "LIMIT")]
        Limit = 1,
        /// <summary>
        /// An order to buy or sell a security at the next available price if the price reaches or surpasses a designated level.
        /// </summary>
        [EnumMember(Value = "STOP")]
        Stop = 2,
        [EnumMember(Value = "STOP_LIMIT")]
        StopLimit = 3,
        /// <summary>
        /// An order to buy or sell a security that automatically adjusts the stop price at a fixed percent or dollar amount below or above the current market price.
        /// </summary>
        [EnumMember(Value = "TRAILING_STOP")]
        TrailingStop = 4,
        [EnumMember(Value = "MARKET_ON_CLOSE")]
        MarketOnClose = 5,
        [EnumMember(Value = "EXERCISE")]
        Exercise = 6,
        [EnumMember(Value = "TRAILING_STOP_LIMIT")]
        TrailingStopLimit = 7,
        /// <summary>
        /// An order where you may select to pay a premium, or net debit. A net debit is the overall amount you're willing to pay.
        /// </summary>
        [EnumMember(Value = "NET_DEBIT")]
        NetDebit = 8,
        /// <summary>
        /// An order where you may select to receive a premium, or net credit. A net credit is the overall amount you want to receive.
        /// </summary>
        [EnumMember(Value = "NET_CREDIT")]
        NetCredit = 9,
        [EnumMember(Value = "NET_ZERO")]
        NetZero = 10
    }
}
