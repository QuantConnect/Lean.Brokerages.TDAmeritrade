using Newtonsoft.Json;
using QuantConnect.Logging;
using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels;
using QuantConnect.TDAmeritrade.Utils.Extensions;
using RestSharp;

namespace QuantConnect.TDAmeritrade.Application
{
    public partial class TDAmeritrade
    {
        public InstrumentModel GetInstrumentByCUSIP(string cusip)
        {
            var request = new RestRequest($"instruments/{cusip}", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);
            request.AddQueryParameter("projection", "fundamental");

            var instrumentResponse = Execute<List<InstrumentModel>>(request);
            return instrumentResponse.First();
        }

        public InstrumentFundamentalModel GetSearchInstruments(string symbol, ProjectionType projectionType = ProjectionType.SymbolSearch)
        {
            var request = new RestRequest("instruments", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);
            request.AddQueryParameter("symbol", symbol);
            request.AddQueryParameter("projection", projectionType.GetProjectionTypeInRequestFormat());

            var instrumentResponse = projectionType switch
            {
                ProjectionType.SymbolSearch => Execute<InstrumentFundamentalModel>(request, symbol),
                ProjectionType.Fundamental => Execute<InstrumentFundamentalModel>(request, symbol),
                _ => throw new ArgumentOutOfRangeException(nameof(projectionType), $"Not expected projectionType value: {projectionType}")
            };

            return instrumentResponse;
        }

        public CandleListModel GetPriceHistory(Symbol symbol, PeriodType periodType = PeriodType.Day,
            int period = 1,
            FrequencyType frequencyType = FrequencyType.NoValue,
            int frequency = 1,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool needExtendedHoursData = true)
        {
            var request = new RestRequest($"marketdata/{symbol.Value}/pricehistory", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);

            ///<example>
            ///Example: For a 2 day / 1 min chart, the values would be:
            ///period: 2
            ///periodType: day
            ///frequency: 1
            ///frequencyType: min
            ///</example>

            if (periodType != PeriodType.Day)
                request.AddQueryParameter("periodType", periodType.GetEnumMemberValue());

            if (IsValidPeriodByPeriodType(periodType, period))
                request.AddQueryParameter("period", period.ToStringInvariant());

            if (IsValidFrequencyTypeByPeriodType(periodType, frequencyType))
                request.AddQueryParameter("frequencyType", frequencyType.GetEnumMemberValue());

            if (IsValidFrequencyByFrequencyType(frequencyType, frequency))
                request.AddQueryParameter("frequency", frequency.ToStringInvariant());

            if (startDate.HasValue && (endDate.HasValue ? endDate.Value > startDate.Value : true))
                request.AddQueryParameter("startDate", Math.Floor(Time.DateTimeToUnixTimeStampMilliseconds(startDate.Value)).ToStringInvariant());

            if (endDate.HasValue && (startDate.HasValue ? startDate.Value < endDate.Value : true))
                request.AddQueryParameter("endDate", Math.Floor(Time.DateTimeToUnixTimeStampMilliseconds(endDate.Value)).ToStringInvariant());

            if (!needExtendedHoursData)
                request.AddQueryParameter("needExtendedHoursData", needExtendedHoursData.ToStringInvariant());

            return Execute<CandleListModel>(request);
        }

        #region TDAmeritrade Helpers

        private bool TryDeserializeRemoveRoot<T>(string json, string rootName, out T obj)
        {
            obj = default;
            var success = false;

            try
            {
                //Dynamic deserialization:
                dynamic dynDeserialized = JsonConvert.DeserializeObject(json);
                obj = JsonConvert.DeserializeObject<T>(dynDeserialized[rootName].ToString());

                // if we arrieved here without exploding it's a success even if obj is null, because that's what we got back
                success = true;
            }
            catch (Exception err)
            {
                Log.Error(err, "RootName: " + rootName);
            }

            return success;
        }

        /// <summary>
        /// Valid periods by periodType (defaults marked with an asterisk)
        /// </summary>
        /// <param name="periodType">day|month|year|ytd</param>
        /// <param name="period">integer value</param>
        /// <example>
        /// periodType.day valid value are 1, 2, 3, 4, 5, 10*
        /// periodType.month: 1*, 2, 3, 6
        /// periodType.year: 1*, 2, 3, 5, 10, 15, 20
        /// periodType.ytd: 1*
        /// </example>
        /// <returns></returns>
        private bool IsValidPeriodByPeriodType(PeriodType periodType, int period)
        {
            var res = periodType switch
            {
                PeriodType.Day when period == 1 || period <= 5 || period == 10 => true,
                PeriodType.Month when period == 1 || period <= 3 || period == 6 => true,
                PeriodType.Year when period == 1 || period <= 3 || period == 5 || period == 10 || period == 15 || period == 20 => true,
                PeriodType.Ytd when period == 1 => true,
                _ => false
            };

            if (!res)
                Log.Error($"TDAmeritrade.Execute.GetPriceHistory(): current perid: {period} doesn't support.");

            return res;
        }

        /// <summary>
        /// Valid frequencyTypes by periodType (defaults marked with an asterisk)
        /// </summary>
        /// <param name="periodType">day|month|year|ytd</param>
        /// <param name="frequencyType">Minute|Daily|Weekly|Monthly</param>
        /// <example>
        /// periodType.day: minute*
        /// periodType.month: daily, weekly*
        /// periodType.year: daily, weekly, monthly*
        /// periodType.ytd: daily, weekly*
        /// </example>
        /// <returns></returns>
        private bool IsValidFrequencyTypeByPeriodType(PeriodType periodType, FrequencyType frequencyType)
        {
            bool res = periodType switch
            {
                PeriodType.Day when frequencyType == FrequencyType.Minute => true,
                PeriodType.Month when
                    frequencyType == FrequencyType.Daily
                    || frequencyType == FrequencyType.Weekly => true,
                PeriodType.Year when
                    frequencyType == FrequencyType.Daily
                    || frequencyType == FrequencyType.Weekly
                    || frequencyType == FrequencyType.Monthly => true,
                PeriodType.Ytd when
                    frequencyType == FrequencyType.Daily
                    || frequencyType == FrequencyType.Weekly => true,
                _ => false
            };

            if (!res)
                Log.Error($"TDAmeritrade.Execute.GetPriceHistory(): current frequencyType: {nameof(frequencyType)} doesn't support.");

            return res;
        }

        private bool IsValidFrequencyByFrequencyType(FrequencyType frequencyType, int frequency)
        {
            bool res = frequencyType switch
            {
                FrequencyType.Minute when frequency == 1
                    || frequency == 5
                    || frequency == 10
                    || frequency == 15
                    || frequency == 30
                    || frequency == 60 => true,
                FrequencyType.Daily when frequency == 1 => true,
                FrequencyType.Weekly when frequency == 1 => true,
                FrequencyType.Monthly when frequency == 1 => true,
                _ => false
            };

            if (!res)
                Log.Error($"TDAmeritrade.Execute.GetPriceHistory(): current frequency: {frequency} doesn't support by frequencyType: {nameof(frequencyType)}");

            return res;
        }

        #endregion
    }
}
