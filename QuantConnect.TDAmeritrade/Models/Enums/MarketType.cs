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
    /// <summary>
    /// The market for which you're requesting market hours. 
    /// Valid markets are EQUITY, OPTION, FUTURE, BOND, or FOREX.
    /// </summary>
    public enum MarketType
    {
        [EnumMember(Value = "EQUITY")]
        EQUITY = 0,
        [EnumMember(Value = "OPTION")]
        OPTION = 1,
        [EnumMember(Value = "FUTURE")]
        FUTURE = 2,
        [EnumMember(Value = "BOND")]
        BOND = 3,
        [EnumMember(Value = "FOREX")]
        FOREX = 4,
    }
}
