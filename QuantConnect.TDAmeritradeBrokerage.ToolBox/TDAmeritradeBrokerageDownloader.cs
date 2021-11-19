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
 *
*/

using QuantConnect.Brokerages.TDAmeritrade;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using TDAmeritradeApi.Client;

namespace QuantConnect.TDAmeritradeDownloader.ToolBox
{
    /// <summary>
    /// Pulls data from TD Ameritrade Brokerage
    /// </summary>
    public class TDAmeritradeBrokerageDownloader : IDataDownloader
    {
        private readonly TDAmeritradeClient client;
        private readonly DateTime _minuteDateStartLimit;
        private readonly Dictionary<Symbol, Dictionary<Resolution, DateRange>> _symbolsToResolutionToDownloadedDataDateRange = new();

        /// <summary>
        /// Initialize <see cref="TDAmeritradeBrokerageDownloader"/>
        /// </summary>
        public TDAmeritradeBrokerageDownloader()
        {
            //Pulls from config file
            client = TDAmeritradeBrokerage.InitializeClient();
            _minuteDateStartLimit = DateTime.UtcNow.AddDays(-45).Date;
        }

        /// <summary>
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        /// </summary>
        /// <param name="symbol">Symbol for the data we're looking for.</param>
        /// <param name="resolution">Resolution of the data request</param>
        /// <param name="startUtc">Start time of the data in UTC</param>
        /// <param name="endUtc">End time of the data in UTC</param>
        /// <returns>Enumerable of base data for this symbol</returns>
        public IEnumerable<BaseData> Get(DataDownloaderGetParameters dataDownloaderGetParameters)
        {
            Symbol symbol = dataDownloaderGetParameters.Symbol;
            Resolution resolution = dataDownloaderGetParameters.Resolution;
            DateTime startUtc = dataDownloaderGetParameters.StartUtc;
            DateTime endUtc = dataDownloaderGetParameters.EndUtc;
            TickType tickType = dataDownloaderGetParameters.TickType;

            if (tickType != TickType.Trade)
                return Enumerable.Empty<BaseData>();


            if (resolution == Resolution.Minute)
            {
                //minute resolution comes in daily increments from LEAN
                // Can only get data 45 days in the past ~1.5 months
                if (startUtc.Date < _minuteDateStartLimit) //wait until valid date
                    return Enumerable.Empty<BaseData>();

                //there is a limit on requests, but not data quantity
                // so get all data up to the future
                endUtc = DateTime.UtcNow;

                if(DataAlreadyDownloaded(symbol, resolution, startUtc))
                    return Enumerable.Empty<BaseData>();
            }

            Log.Trace($"Downloading {resolution} data for {symbol} from {startUtc} to present");

            var history = TDAmeritradeBrokerage.GetPriceHistory(client, symbol, resolution, startUtc, endUtc, TimeZones.NewYork);

            UpdateDownloadedDateRange(symbol, resolution, history);

            return history;
        }

        private bool DataAlreadyDownloaded(Symbol symbol, Resolution resolution, DateTime startUtc)
        {
            return _symbolsToResolutionToDownloadedDataDateRange.ContainsKey(symbol) &&
                _symbolsToResolutionToDownloadedDataDateRange[symbol].ContainsKey(resolution) &&
                _symbolsToResolutionToDownloadedDataDateRange[symbol][resolution].Start < startUtc &&
                _symbolsToResolutionToDownloadedDataDateRange[symbol][resolution].End > startUtc;
        }

        private void UpdateDownloadedDateRange(Symbol symbol, Resolution resolution, IEnumerable<TradeBar> history)
        {
            var startDateTime = history.First().Time;
            //subtract off a day so we always grab the latest day
            var endDateTime = history.Last().Time.Date.AddDays(-1);

            if (!_symbolsToResolutionToDownloadedDataDateRange.ContainsKey(symbol))
            {
                _symbolsToResolutionToDownloadedDataDateRange.Add(symbol, new());
            }

            var resolutionToRange = _symbolsToResolutionToDownloadedDataDateRange[symbol];

            if (!resolutionToRange.ContainsKey(resolution))
            {
                resolutionToRange.Add(resolution, new(startDateTime, endDateTime));
            }
            else
            {
                var range = resolutionToRange[resolution];

                if (startDateTime < range.Start)
                {
                    resolutionToRange[resolution].Start = startDateTime;
                }
                if (range.End < endDateTime)
                {
                    resolutionToRange[resolution].End = endDateTime;
                }
            }
        }

        private class DateRange
        {
            public DateTime Start { get; set; }

            public DateTime End { get; set; }

            public DateRange(DateTime startDateTime, DateTime endDateTime)
            {
                Start = startDateTime;
                End = endDateTime;
            }
        }
    }
}
