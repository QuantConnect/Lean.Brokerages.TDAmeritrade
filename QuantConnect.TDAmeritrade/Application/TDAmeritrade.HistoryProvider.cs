using NodaTime;
using QuantConnect.Data;
using QuantConnect.Logging;

namespace QuantConnect.TDAmeritrade.Application
{
    /// <summary>
    /// TDAmeritrade - IHistoryProvider implementation
    /// </summary>
    public partial class TDAmeritrade
    {
        private bool _loggedTradierSupportsOnlyTradeBars;

        /// <summary>
        /// Gets the history for the requested security
        /// </summary>
        /// <param name="request">The historical data request</param>
        /// <returns>An enumerable of bars covering the span specified in the request</returns>
        public override IEnumerable<BaseData> GetHistory(HistoryRequest request)
        {
            if (request.Symbol.ID.SecurityType != SecurityType.Equity && request.Symbol.ID.SecurityType != SecurityType.Option)
            {
                throw new ArgumentException($"Invalid security type: {request.Symbol.ID.SecurityType}");
            }

            if (request.StartTimeUtc >= request.EndTimeUtc)
            {
                throw new ArgumentException("Invalid date range specified start time can not be after end time");
            }

            if (request.Symbol.IsCanonical())
            {
                throw new ArgumentException("Invalid symbol, cannot use canonical symbols for history request");
            }

            if (request.TickType != TickType.Trade)
            {
                if (!_loggedTradierSupportsOnlyTradeBars)
                {
                    _loggedTradierSupportsOnlyTradeBars = true;
                    _algorithm?.Debug("Warning: Tradier history provider only supports trade information, does not support quotes.");
                    Log.Error("TradierBrokerage.GetHistory(): Tradier only supports TradeBars");
                }
                yield break;
            }

            var start = request.StartTimeUtc.ConvertTo(DateTimeZone.Utc, TimeZones.NewYork);
            var end = request.EndTimeUtc.ConvertTo(DateTimeZone.Utc, TimeZones.NewYork);


            IEnumerable<BaseData> history;
            switch (request.Resolution)
            {
                //case Resolution.Tick:
                //    history = GetHistoryTick(request, start, end);
                //    break;

                //case Resolution.Second:
                //    history = GetHistorySecond(request, start, end);
                //    break;

                case Resolution.Minute:
                    //history = GetHistoryMinute(request, start, end);
                    break;

                case Resolution.Hour:
                    //history = GetHistoryHour(request, start, end);
                    break;

                case Resolution.Daily:
                    //history = GetHistoryDaily(request, start, end);
                    break;

                default:
                    throw new ArgumentException("Invalid date range specified");
            }

            //foreach (var bar in history.Where(bar => bar.Time >= request.StartTimeLocal && bar.EndTime <= request.EndTimeLocal))
            //{
            //    if (request.ExchangeHours.IsOpen(bar.Time, bar.EndTime, request.IncludeExtendedMarketHours))
            //    {
            //        yield return bar;
            //    }
            //}
        }

        //private IEnumerable<BaseData> GetHistoryMinute(HistoryRequest request, DateTime start, DateTime end)
        //{
        //    var symbol = request.Symbol;
        //    var exchangeTz = request.ExchangeHours.TimeZone;
        //    var requestedBarSpan = request.Resolution.ToTimeSpan();
        //    var history = GetTimeSeries(symbol, start, end, TradierTimeSeriesIntervals.OneMinute);

        //    if (history == null)
        //    {
        //        return Enumerable.Empty<BaseData>();
        //    }

        //    return history.Select(bar => new TradeBar(bar.Time, symbol, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume, requestedBarSpan));
        //}
    }
}
