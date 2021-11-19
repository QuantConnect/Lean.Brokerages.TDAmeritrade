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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Util;
using TDAmeritradeApi.Client;
using TDAmeritradeApi.Client.Models.MarketData;
using HistoryRequest = QuantConnect.Data.HistoryRequest;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    /// <summary>
    /// TD Ameritrade Brokerage - IHistoryProvider implementation
    /// </summary>
    public partial class TDAmeritradeBrokerage : IHistoryProvider
    {
        #region IHistoryProvider implementation

        /// <summary>
        /// Event fired when an invalid configuration has been detected
        /// </summary>
        public event EventHandler<InvalidConfigurationDetectedEventArgs> InvalidConfigurationDetected;

        /// <summary>
        /// Event fired when the numerical precision in the factor file has been limited
        /// </summary>
        public event EventHandler<NumericalPrecisionLimitedEventArgs> NumericalPrecisionLimited;

        /// <summary>
        /// Event fired when there was an error downloading a remote file
        /// </summary>
        public event EventHandler<DownloadFailedEventArgs> DownloadFailed;

        /// <summary>
        /// Event fired when there was an error reading the data
        /// </summary>
        public event EventHandler<ReaderErrorDetectedEventArgs> ReaderErrorDetected;

        /// <summary>
        /// Event fired when the start date has been limited
        /// </summary>
#pragma warning disable 0067 // StartDateLimited is currently not used; remove once implemented
        public event EventHandler<StartDateLimitedEventArgs> StartDateLimited;
#pragma warning restore 0067

        /// <summary>
        /// Gets the total number of data points emitted by this history provider
        /// </summary>
        public int DataPointCount { get; private set; }

        /// <summary>
        /// Initializes this history provider to work for the specified job
        /// </summary>
        /// <param name="parameters">The initialization parameters</param>
        public void Initialize(HistoryProviderInitializeParameters parameters)
        {
        }

        /// <summary>
        /// Gets the history for the requested security
        /// </summary>
        /// <param name="request">The historical data request</param>
        /// <returns>An enumerable of bars covering the span specified in the request</returns>
        public override IEnumerable<BaseData> GetHistory(HistoryRequest request)
        {
            if (request.TickType != TickType.Trade)
            {
                yield break;
            }

            var history = GetPriceHistory(_tdClient, request.Symbol, request.Resolution, request.StartTimeUtc, request.EndTimeUtc, TimeZones.NewYork);

            DataPointCount += history.Count();

            foreach (var slice in TradeBarsToSlices(history))
            {
                yield return slice[request.Symbol];
            }
        }

        /// <summary>
        /// Gets the history for the requested securities
        /// </summary>
        /// <param name="requests">The historical data requests</param>
        /// <param name="sliceTimeZone">The time zone used when time stamping the slice instances</param>
        /// <returns>An enumerable of the slices of data covering the span specified in each request</returns>
        public IEnumerable<Slice> GetHistory(IEnumerable<HistoryRequest> requests, DateTimeZone sliceTimeZone)
        {
            foreach (var request in requests)
            {
                IEnumerable<TradeBar> history;
                if (request.TickType == TickType.Trade)
                {
                    history = GetPriceHistory(_tdClient, request.Symbol, request.Resolution, request.StartTimeUtc, request.EndTimeUtc, sliceTimeZone);
                }
                else
                {
                    history = Enumerable.Empty<TradeBar>();
                }

                DataPointCount += history.Count();

                foreach (var slice in TradeBarsToSlices(history))
                {
                    yield return slice;
                }
            }
        }

        /// <summary>
        /// Method for getting price history
        /// </summary>
        /// <param name="symbol">symbol to get data for</param>
        /// <param name="resolution">time period of data</param>
        /// <param name="startDate">from date</param>
        /// <param name="endDate">to date</param>
        /// <returns>history</returns>
        public static IEnumerable<TradeBar> GetPriceHistory(TDAmeritradeClient tdClient, Symbol symbol, Resolution resolution, DateTime startDate, DateTime endDate, DateTimeZone sliceTimeZone)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Invalid date range specified");
            }

            var start = startDate;//.ConvertTo(DateTimeZone.Utc, TimeZones.NewYork);
            var end = endDate;//.ConvertTo(DateTimeZone.Utc, TimeZones.NewYork);

            var history = Enumerable.Empty<TradeBar>();

            lock (_apiClientLock)
            {
                if (_nonOrderRateGate.IsRateLimited)
                {
                    _nonOrderRateGate.WaitToProceed();
                }

                try
                {
                    switch (resolution)
                    {
                        case Resolution.Tick:
                        case Resolution.Second:
                            break;

                        case Resolution.Minute:
                            history = GetHistoryMinute(tdClient, symbol, start, end);
                            break;

                        case Resolution.Hour:
                            history = GetHistoryHour(tdClient, symbol, start, end);
                            break;

                        case Resolution.Daily:
                            history = GetHistoryDaily(tdClient, symbol, start, end);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                if (sliceTimeZone != TimeZones.NewYork)
                {
                    history.DoForEach(bar =>
                    {
                        bar.Time = bar.Time.ConvertTo(TimeZones.NewYork, sliceTimeZone);
                        bar.EndTime = bar.EndTime.ConvertTo(TimeZones.NewYork, sliceTimeZone);
                    });
                }

                return history;
            }
        }

        /// <summary>
        /// Event invocator for the <see cref="InvalidConfigurationDetected"/> event
        /// </summary>
        /// <param name="e">Event arguments for the <see cref="InvalidConfigurationDetected"/> event</param>
        protected virtual void OnInvalidConfigurationDetected(InvalidConfigurationDetectedEventArgs e)
        {
            InvalidConfigurationDetected?.Invoke(this, e);
        }

        /// <summary>
        /// Event invocator for the <see cref="NumericalPrecisionLimited"/> event
        /// </summary>
        /// <param name="e">Event arguments for the <see cref="NumericalPrecisionLimited"/> event</param>
        protected virtual void OnNumericalPrecisionLimited(NumericalPrecisionLimitedEventArgs e)
        {
            NumericalPrecisionLimited?.Invoke(this, e);
        }

        /// <summary>
        /// Event invocator for the <see cref="DownloadFailed"/> event
        /// </summary>
        /// <param name="e">Event arguments for the <see cref="DownloadFailed"/> event</param>
        protected virtual void OnDownloadFailed(DownloadFailedEventArgs e)
        {
            DownloadFailed?.Invoke(this, e);
        }

        /// <summary>
        /// Event invocator for the <see cref="ReaderErrorDetected"/> event
        /// </summary>
        /// <param name="e">Event arguments for the <see cref="ReaderErrorDetected"/> event</param>
        protected virtual void OnReaderErrorDetected(ReaderErrorDetectedEventArgs e)
        {
            ReaderErrorDetected?.Invoke(this, e);
        }

        /// <summary>
        /// Get history data by minute resolution
        /// </summary>
        /// <param name="tdClient">td ameritrade api client</param>
        /// <param name="symbol">symbol to get data for</param>
        /// <param name="start">from date</param>
        /// <param name="end">to date</param>
        /// <returns>historic minute data</returns>
        private static IEnumerable<TradeBar> GetHistoryMinute(TDAmeritradeClient tdClient, Symbol symbol, DateTime start, DateTime end)
        {
            string brokerageSymbol = TDAmeritradeToLeanMapper.GetBrokerageSymbol(symbol);

            var history = tdClient.MarketDataApi.GetPriceHistoryAsync(brokerageSymbol, frequencyType: FrequencyType.minute, frequency: 1, startDate: new DateTimeOffset(start), endDate: new DateTimeOffset(end)).Result;

            return CandlesToTradeBars(symbol, history, Time.OneMinute);
        }

        /// <summary>
        /// Convert TD Ameritrade API CandleList object to LEAN slices
        /// </summary>
        /// <param name="symbol">symbol that data belongs to</param>
        /// <param name="history">TD Ameritrade API CandleList</param>
        /// <param name="time">period frequency in <see cref="System.TimeSpan"/></param>
        /// <returns>slices</returns>
        private IEnumerable<Slice> CandlesToSlices(Symbol symbol, CandleList history, TimeSpan time)
        {
            var tradeBars = CandlesToTradeBars(symbol, history, time);

            return TradeBarsToSlices(tradeBars);
        }

        /// <summary>
        /// Converts trade bars to slices
        /// </summary>
        /// <param name="tradeBars">historic trade bars</param>
        /// <returns>slices</returns>
        private static IEnumerable<Slice> TradeBarsToSlices(IEnumerable<TradeBar> tradeBars)
        {
            if (tradeBars == null || !tradeBars.Any())
                return Enumerable.Empty<Slice>();

            return tradeBars.Select(tradeBar => new Slice(tradeBar.EndTime, new[] { tradeBar }));
        }

        /// <summary>
        /// Convert TD Ameritrade API CandleList object to LEAN trade bars
        /// </summary>
        /// <param name="symbol">symbol that data belongs to</param>
        /// <param name="history">TD Ameritrade API CandleList</param>
        /// <param name="time">period frequency in <see cref="System.TimeSpan"/></param>
        /// <returns>slices</returns>
        private static IEnumerable<TradeBar> CandlesToTradeBars(Symbol symbol, CandleList history, TimeSpan time)
        {
            if (history == null)
                return Enumerable.Empty<TradeBar>();

            return history.candles
                            .Select(candle => new TradeBar(candle.datetime, symbol, candle.open, candle.high, candle.low, candle.close, candle.volume, time));
        }

        /// <summary>
        /// Get history data by hour resolution
        /// </summary>
        /// <param name="tdClient">td ameritrade api client</param>
        /// <param name="symbol">symbol to get data for</param>
        /// <param name="start">from date</param>
        /// <param name="end">to date</param>
        /// <returns>historic hour data</returns>
        private static IEnumerable<TradeBar> GetHistoryHour(TDAmeritradeClient tdClient, Symbol symbol, DateTime start, DateTime end)
        {
            string brokerageSymbol = TDAmeritradeToLeanMapper.GetBrokerageSymbol(symbol);

            var history = tdClient.MarketDataApi.GetPriceHistoryAsync(brokerageSymbol, frequencyType: FrequencyType.minute, frequency: 30, startDate: new DateTimeOffset(start), endDate: new DateTimeOffset(end)).Result;

            var tradeBars = CandlesToTradeBars(symbol, history, Time.OneHour);

            var aggregatedBars = tradeBars
                .GroupBy(x => x.Time.RoundDown(Time.OneHour))
                .Select(g => new TradeBar(
                    g.Key,
                    symbol,
                    g.First().Open,
                    g.Max(t => t.High),
                    g.Min(t => t.Low),
                    g.Last().Close,
                    g.Sum(t => t.Volume),
                    Time.OneHour));

            return aggregatedBars;
        }

        /// <summary>
        /// Get history data by daily resolution
        /// </summary>
        /// <param name="tdClient">td ameritrade api client</param>
        /// <param name="symbol">symbol to get data for</param>
        /// <param name="start">from date</param>
        /// <param name="end">to date</param>
        /// <returns>historic daily data</returns>
        private static IEnumerable<TradeBar> GetHistoryDaily(TDAmeritradeClient tdClient, Symbol symbol, DateTime start, DateTime end)
        {
            string brokerageSymbol = TDAmeritradeToLeanMapper.GetBrokerageSymbol(symbol);

            var history = tdClient.MarketDataApi.GetPriceHistoryAsync(brokerageSymbol, periodType: PeriodType.year, frequencyType: FrequencyType.daily, frequency: 1, startDate: new DateTimeOffset(start), endDate: new DateTimeOffset(end)).Result;

            //this fixes a bug in the td data that returns utc from chicago time when all other data is from new york time
            Array.ForEach(history.candles, candle => candle.datetime = candle.datetime.Date);

            return CandlesToTradeBars(symbol, history, Time.OneDay);
        }

        #endregion
    }
}
