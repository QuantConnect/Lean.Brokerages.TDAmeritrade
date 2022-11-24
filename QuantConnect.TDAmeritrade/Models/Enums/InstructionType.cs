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
    public enum InstructionType
    {
        [EnumMember(Value = "BUY")]
        Buy = 0,
        [EnumMember(Value = "SELL")]
        Sell = 1,
        [EnumMember(Value = "BUY_TO_COVER")]
        BuyToCover = 2,
        [EnumMember(Value = "BUY_TO_OPEN")]
        BuyToOpen = 3,
        [EnumMember(Value = "BUY_TO_CLOSE")]
        BuyToClose = 4,
        [EnumMember(Value = "SELL_TO_OPEN")]
        SellToOpen = 5,
        [EnumMember(Value = "SELL_TO_CLOSE")]
        SellToClose = 6,
        [EnumMember(Value = "EXCHANGE")]
        Exchange = 7,
    }
}
