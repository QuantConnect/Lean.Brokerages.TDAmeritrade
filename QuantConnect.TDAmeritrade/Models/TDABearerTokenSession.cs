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
    /// <summary>
    /// Create a new Bearer token session
    /// </summary>
    public class TDABearerTokenSession
    {
        /// <summary>
        /// Bearer Authorization token is lived for 1800 seconds (30 minutes)
        /// </summary>
        private readonly static TimeSpan LifeSpan = TimeSpan.FromSeconds(1730);
        /// <summary>
        /// Bearer Token creating time
        /// </summary>
        private readonly DateTime _createdTime;

        /// <summary>
        /// TDAmeritrade Bearer Token
        /// </summary>
        public readonly string BearerToken;

        /// <summary>
        /// Determines if this bearer token session is valid
        /// </summary>
        public bool IsValid => DateTime.UtcNow - _createdTime < LifeSpan;

        public TDABearerTokenSession(string bearerToken)
        {
            _createdTime = DateTime.UtcNow;
            BearerToken = bearerToken;
        }
    }
}
