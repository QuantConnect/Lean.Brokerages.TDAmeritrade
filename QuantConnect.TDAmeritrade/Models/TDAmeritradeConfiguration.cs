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

using QuantConnect.Configuration;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public static class TDAmeritradeConfiguration
    {
        /// <summary>
        /// Get from TD Ameritrade developer account
        /// </summary>
        public static string ConsumerKey => Config.Get("tdameritrade-consumer-key");

        /// <summary>
        /// Get from TD Ameritrade developer account (Callback URL)
        /// </summary>
        public static string CallbackUrl => Config.Get("tdameritrade-callback-url");

        /// <summary>
        /// Get from TD Ameritrade broker account
        /// <see href="https://developer.tdameritrade.com/content/authentication-faq"/>
        /// <seealso href="https://www.reddit.com/r/algotrading/comments/c81vzq/td_ameritrade_api_access_2019_guide/"/>
        /// </summary>
        public static string AccessToken => Config.Get("tdameritrade-code-from-url");

        /// <summary>
        /// Get from authorization code (A refresh token is valid for 90 days)
        /// </summary>
        public static string RefreshToken => Config.Get("tdameritrade-refresh-token");

        /// <summary>
        /// Get from TD Ameritrade brokerage account
        /// </summary>
        public static string AccountNumber => Config.Get("tdameritrade-account-number");
    }
}
