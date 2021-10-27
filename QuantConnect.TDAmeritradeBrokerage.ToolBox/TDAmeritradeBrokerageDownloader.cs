﻿/*
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
 *
*/

using QuantConnect.Brokerages.TDAmeritrade;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using TDAmeritradeApi.Client;

namespace QuantConnect.TDAmeritradeDownloader.ToolBox
{
    /// <summary>
    /// Pulls data from TD Ameritrade Brokerage
    /// </summary>
    public class TDAmeritradeBrokerageDownloader : IDataDownloader
    {
        private readonly TDAmeritradeClient client;

        /// <summary>
        /// Initialize <see cref="TDAmeritradeBrokerageDownloader"/>
        /// </summary>
        public TDAmeritradeBrokerageDownloader()
        {
            //Pulls from config file
            client = TDAmeritradeBrokerage.InitializeClient();
        }

        /// <summary>
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        /// </summary>
        /// <param name="symbol">Symbol for the data we're looking for.</param>
        /// <param name="resolution">Resolution of the data request</param>
        /// <param name="startUtc">Start time of the data in UTC</param>
        /// <param name="endUtc">End time of the data in UTC</param>
        /// <returns>Enumerable of base data for this symbol</returns>
        public IEnumerable<BaseData> Get(Symbol symbol, Resolution resolution, DateTime startUtc, DateTime endUtc)
        {
            return TDAmeritradeBrokerage.GetPriceHistory(client, symbol, resolution, startUtc, endUtc, TimeZones.NewYork);
        }
    }
}
