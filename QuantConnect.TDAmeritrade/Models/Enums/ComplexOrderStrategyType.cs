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
    public enum ComplexOrderStrategyType
    {
        [EnumMember(Value = "NONE")]
        None = 0,
        [EnumMember(Value = "COVERED")]
        Covered = 1,
        [EnumMember(Value = "VERTICAL")]
        Vertical = 2,
        [EnumMember(Value = "BACK_RATIO")]
        BackRatio = 3,
        [EnumMember(Value = "CALENDAR")]
        Calendar = 4,
        [EnumMember(Value = "DIAGONAL")]
        Diagonal = 5,
        [EnumMember(Value = "STRADDLE")]
        Straddle = 6,
        [EnumMember(Value = "STRANGLE")]
        Strangle = 7,
        [EnumMember(Value = "COLLAR_SYNTHETIC")]
        CollarSynthetic = 8,
        [EnumMember(Value = "BUTTERFLY")]
        Butterfly = 9,
        [EnumMember(Value = "CONDOR")]
        Condor = 10,
        [EnumMember(Value = "IRON_CONDOR")]
        IronCondor = 11,
        [EnumMember(Value = "VERTICAL_ROLL")]
        VerticalRoll = 12,
        [EnumMember(Value = "COLLAR_WITH_STOCK")]
        CollarWithStock = 13,
        [EnumMember(Value = "DOUBLE_DIAGONAL")]
        DoubleDiagonal = 14,
        [EnumMember(Value = "UNBALANCED_BUTTERFLY")]
        UnbalancedButterfly = 15,
        [EnumMember(Value = "UNBALANCED_CONDOR")]
        UnbalancedCondor= 16,
        [EnumMember(Value = "UNBALANCED_IRON_CONDOR")]
        UnbalancedIronCondor = 17,
        [EnumMember(Value = "UNBALANCED_VERTICAL_ROLL")]
        UnbalancedVerticalRoll = 18,
        [EnumMember(Value = "CUSTOM")]
        Custom = 19,
    }
}
