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
    public enum TransactionType
    {
        [EnumMember(Value = "No Value")]
        NO_VALUE = 0,
        [EnumMember(Value = "ALL")]
        ALL = 1,
        [EnumMember(Value = "TRADE")]
        TRADE = 2,
        [EnumMember(Value = "BUY_ONLY")]
        BUY_ONLY = 3,
        [EnumMember(Value = "SELL_ONLY")]
        SELL_ONLY = 4,
        [EnumMember(Value = "CASH_IN_OR_CASH_OUT")]
        CASH_IN_OR_CASH_OUT = 5,
        [EnumMember(Value = "CHECKING")]
        CHECKING = 6,
        [EnumMember(Value = "DIVIDEND")]
        DIVIDEND = 7,
        [EnumMember(Value = "INTEREST")]
        INTEREST = 8,
        [EnumMember(Value = "OTHER")]
        OTHER = 9,
        [EnumMember(Value = "ADVISOR_FEES")]
        ADVISOR_FEES = 10,
    }
}
