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
    public enum OrderStatusType
    {
        NoValue = 0,
        AwaitingParentOrder = 1,
        AwaitingCondition = 2,
        AwaitingManualReview = 3,
        Accepted = 4,
        AwaitingurOut = 5,
        PendingActivation = 6,
        Queued = 7,
        Working = 8,
        Rejected = 9,
        PendingCancel = 10,
        Canceled = 11,
        PendingReplace = 12,
        Replaced = 13,
        Filled = 14,
        Expired = 15,
    }
}
