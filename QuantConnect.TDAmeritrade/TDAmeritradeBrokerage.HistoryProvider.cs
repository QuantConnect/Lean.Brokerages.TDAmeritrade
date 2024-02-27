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

using NodaTime;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Brokerages.TDAmeritrade.Utils;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        private bool _loggedTDASupportsOnlyTradeBars;
        private bool _loggedTDAUnsupportedSecurityTypeForHistory;
        private bool _loggedInvalidTimeRangeForHistory;
        private bool _loggedInvalidCanonicalSymbolForHistory;
        private bool _loggedTDAUnsupportedResolutionForHistory;

        /// <summary>
        /// Gets the history for the requested security
        /// </summary>
        /// <param name="request">The historical data request</param>
        /// <returns>An enumerable of bars covering the span specified in the request</returns>
        public override IEnumerable<BaseData>? GetHistory(HistoryRequest request)
        {
            if (request.Symbol.ID.SecurityType != SecurityType.Equity)
            {
                if (!_loggedTDAUnsupportedSecurityTypeForHistory)
                {
                    _loggedTDAUnsupportedSecurityTypeForHistory = true;
                    _algorithm?.Debug("Warning: TDAmeritrade history provider only supports equities.");
                    Log.Error("TDAmeritradeBrokerage.GetHistory(): TDAmeritrade only supports equities");
                }
                return null;
            }

            if (request.StartTimeUtc >= request.EndTimeUtc)
            {
                if (!_loggedInvalidTimeRangeForHistory)
                {
                    _loggedInvalidTimeRangeForHistory = true;
                    _algorithm?.Debug("Warning: TDAmeritrade history provider received an invalid date range for history request.");
                    Log.Error("TDAmeritradeBrokerage.GetHistory(): Invalid date range specified");
                }
                return null;
            }

            if (request.Symbol.IsCanonical())
            {
                if (!_loggedInvalidCanonicalSymbolForHistory)
                {
                    _loggedInvalidCanonicalSymbolForHistory = true;
                    _algorithm?.Debug("Warning: TDAmeritrade history provider received an invalid symbol for history request.");
                    Log.Error("TDAmeritradeBrokerage.GetHistory(): Invalid symbol, cannot use canonical symbols for history request");
                }
                return null;
            }

            if (request.TickType != TickType.Trade)
            {
                if (!_loggedTDASupportsOnlyTradeBars)
                {
                    _loggedTDASupportsOnlyTradeBars = true;
                    _algorithm?.Debug("Warning: TDAmeritrade history provider only supports trade information, does not support quotes.");
                    Log.Error("TDAmeritradeBrokerage.GetHistory(): TDAmeritrade only supports TradeBars");
                }
                return null;
            }

            if (request.Resolution < Resolution.Minute)
            {
                if (!_loggedTDAUnsupportedResolutionForHistory)
                {
                    _loggedTDAUnsupportedResolutionForHistory = true;
                    _algorithm?.Debug("Warning: TDAmeritrade history provider only supports minute and higher resolution.");
                    Log.Error("TDAmeritradeBrokerage.GetHistory(): TDAmeritrade only supports minute and higher resolution");
                }
                return null;
            }

            var start = request.StartTimeUtc.ConvertTo(DateTimeZone.Utc, TimeZones.NewYork);
            var end = request.EndTimeUtc.ConvertTo(DateTimeZone.Utc, TimeZones.NewYork);

            IEnumerable<BaseData> history = default;

            switch (request.Resolution)
            {
                case Resolution.Minute:
                    history = GetHistory(request, start, end, PeriodType.Day, FrequencyType.Minute);
                    break;

                case Resolution.Hour:
                    history = GetHistory(request, start, end, PeriodType.Day, FrequencyType.Minute);
                    break;

                case Resolution.Daily:
                    history = GetHistory(request, start, end, PeriodType.Year, FrequencyType.Daily);
                    break;

                default:
                    throw new ArgumentException("Invalid date range specified");
            }

            return history.Where(bar => bar.Time >= request.StartTimeLocal &&
                bar.EndTime <= request.EndTimeLocal &&
                request.ExchangeHours.IsOpen(bar.Time, bar.EndTime, request.IncludeExtendedMarketHours));
        }

        private IEnumerable<BaseData> GetHistory(HistoryRequest request, DateTime start, DateTime end, PeriodType periodType, FrequencyType frequencyType)
        {
            var symbol = request.Symbol;
            var exchangeTz = request.ExchangeHours.TimeZone;
            var requestedBarSpan = request.Resolution.ToTimeSpan();

            var history = GetPriceHistory(
                symbol,
                periodType,
                1,
                frequencyType: frequencyType,
                frequency: request.Resolution.ResolutionToFrequency(),
                startDate: start,
                endDate: end,
                true);

            if (history == null)
            {
                return Enumerable.Empty<BaseData>();
            }

            return history.Candles.Select(candle =>
                new TradeBar(
                    Time.UnixMillisecondTimeStampToDateTime(candle.DateTime),
                    symbol,
                    candle.Open,
                    candle.High,
                    candle.Low,
                    candle.Close,
                    candle.Volume,
                    requestedBarSpan
                 ));
        }
    }
}
