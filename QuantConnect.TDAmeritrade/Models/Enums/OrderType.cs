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

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum OrderType
    {
        /// <summary>
        /// An order seeking execution of a buy or sell transaction immediately at the next available market price. 
        /// </summary>
        Market = 0,
        /// <summary>
        /// An order seeking execution of a buy or sell transaction at a specified price or better.
        /// </summary>
        Limit = 1,
        /// <summary>
        /// An order to buy or sell a security at the next available price if the price reaches or surpasses a designated level.
        /// </summary>
        Stop = 2,
        StopLimit = 3,
        /// <summary>
        /// An order to buy or sell a security that automatically adjusts the stop price at a fixed percent or dollar amount below or above the current market price.
        /// </summary>
        TrailingStop = 4,
        MarketOnClose = 5,
        Exercise = 6,
        TrailingStopLimit = 7,
        /// <summary>
        /// An order where you may select to pay a premium, or net debit. A net debit is the overall amount you're willing to pay.
        /// </summary>
        NetDebit = 8,
        /// <summary>
        /// An order where you may select to receive a premium, or net credit. A net credit is the overall amount you want to receive.
        /// </summary>
        NetCredit = 9,
        NetZero = 10
    }
}
