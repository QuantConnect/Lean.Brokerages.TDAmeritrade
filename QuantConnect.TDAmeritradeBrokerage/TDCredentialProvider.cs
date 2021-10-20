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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDAmeritradeApi.Client;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    /// <summary>
    /// TD Ameritrade API credential provider for inputting information by the command line.
    /// </summary>
    public class TDCliCredentialProvider : ICredentials
    {
        /// <summary>
        /// Callback method for getting the password from the command line.
        /// </summary>
        /// <returns>password</returns>
        public string GetPassword()
        {
            Console.WriteLine("Password: ");

            return Console.ReadLine();
        }

        /// <summary>
        /// Callback method for getting the multi-factor authorization code from the command line.
        /// </summary>
        /// <returns>code</returns>
        public string GetSmsCode()
        {
            Console.WriteLine("Sms code: ");

            return Console.ReadLine();
        }

        /// <summary>
        /// Callback method for getting the username from the command line.
        /// </summary>
        /// <returns>username</returns>
        public string GetUserName()
        {
            Console.WriteLine("Username: ");

            return Console.ReadLine();
        }
    }
}
