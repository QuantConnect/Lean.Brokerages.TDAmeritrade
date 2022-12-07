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
    public enum ComplexOrderStrategyType
    {
        None = 0,
        Covered = 1,
        Vertical = 2,
        BackRatio = 3,
        Calendar = 4,
        Diagonal = 5,
        Straddle = 6,
        Strangle = 7,
        CollarSynthetic = 8,
        Butterfly = 9,
        Condor = 10,
        IronCondor = 11,
        VerticalRoll = 12,
        CollarWithStock = 13,
        DoubleDiagonal = 14,
        UnbalancedButterfly = 15,
        UnbalancedCondor = 16,
        UnbalancedIronCondor = 17,
        UnbalancedVerticalRoll = 18,
        Custom = 19,
    }
}
