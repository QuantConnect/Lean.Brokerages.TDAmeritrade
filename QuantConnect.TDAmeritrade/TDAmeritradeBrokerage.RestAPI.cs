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

using Newtonsoft.Json;
using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Brokerages.TDAmeritrade.Utils;
using QuantConnect.Logging;
using RestSharp;
using System.Net;
using System.Web;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeBrokerage
    {
        #region GET

        /// <summary>
        /// Get price history for a symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="periodType">The type of period to show. Valid values are day, month, year, or ytd (year to date)</param>
        /// <param name="period">The number of periods to show. Example: For a 2 day / 1 min chart, the values would be</param>
        /// <param name="frequencyType">
        /// The type of frequency with which a new candle is formed. 
        /// day: minute*
        /// month: daily, weekly*
        /// year: daily, weekly, monthly*
        /// ytd: daily, weekly*
        /// </param>
        /// <param name="frequency">The number of the frequencyType to be included in each candle.
        /// minute: 1*, 5, 10, 15, 30
        /// daily: 1*
        /// weekly: 1*
        /// monthly: 1*
        /// </param>
        /// <param name="startDate">Start date as milliseconds since epoch. If startDate and endDate are provided, period should not be provided.</param>
        /// <param name="endDate">End date as milliseconds since epoch. If startDate and endDate are provided, period should not be provided. Default is previous trading day.</param>
        /// <param name="needExtendedHoursData">true to return extended hours data, false for regular market hours only.</param>
        /// <returns></returns>
        private CandleListModel GetPriceHistory(
            Symbol symbol, 
            PeriodType periodType,
            int period,
            FrequencyType frequencyType,
            int frequency,
            DateTime? startDate,
            DateTime? endDate,
            bool needExtendedHoursData)
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

        // Quotes
        /// <summary>
        /// Get quote for a symbol
        /// </summary>
        public QuoteTDAmeritradeModel GetQuote(string symbol)
        {
            var request = new RestRequest($"marketdata/{symbol}/quotes", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);

            return Execute<QuoteTDAmeritradeModel>(request, symbol);
        }

        /// <summary>
        /// Get quote for one or more symbols
        /// </summary>
        public IEnumerable<QuoteTDAmeritradeModel> GetQuotes(params string[] symbols)
        {
            var request = new RestRequest("marketdata/quotes", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);

            var symbolsInOneLine = string.Join(",", symbols);

            request.AddQueryParameter("symbol", symbolsInOneLine);

            var jsonResponse = Execute<string>(request);

            var qutes = new List<QuoteTDAmeritradeModel>(symbols.Length);

            foreach (var symbol in symbols)
            {
                if (TryDeserializeRemoveRoot(jsonResponse, symbol, out QuoteTDAmeritradeModel result))
                    qutes.Add(result);
            }

            return qutes;
        }

        /// <summary>
        /// User have to redirect by this url to copy code from url
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <see href="https://www.reddit.com/r/algotrading/comments/c81vzq/td_ameritrade_api_access_2019_guide/"/>
        /// <seealso href="https://www.reddit.com/r/algotrading/comments/914q22/successful_access_to_td_ameritrade_api/"/>
        /// <returns></returns>
        public string GetSignInUrl(string redirectUrl = "http://localhost")
        {
            var encodedKey = HttpUtility.UrlEncode(_consumerKey);
            var encodedUri = HttpUtility.UrlEncode(redirectUrl);
            var path = $"https://auth.tdameritrade.com/auth?response_type=code&redirect_uri={encodedUri}&client_id={encodedKey}%40AMER.OAUTHAP";
            return path;
        }

        /// <summary>
        /// Account balances, positions, and orders for a specific account.
        /// </summary>
        public AccountModel GetAccount(string accountNumber)
        {
            var request = new RestRequest($"accounts/{accountNumber}", Method.GET);

            request.AddQueryParameter("fields", "positions,orders");

            return Execute<AccountModel>(request);
        }

        /// <summary>
        /// Orders for a specific account.
        /// </summary>
        /// <param name="maxResults">The max number of orders to retrieve.</param>
        /// <param name="fromEnteredTime">Specifies that no orders entered before this time should be returned. 
        /// Valid ISO-8601 formats are :yyyy-MM-dd. 
        /// If 'toEnteredTime' is not sent, the default `toEnteredTime` would be the current day.
        /// </param>
        /// <param name="toEnteredTime">Specifies that no orders entered after this time should be returned.
        /// Valid ISO-8601 formats are :yyyy-MM-dd. 
        /// If 'fromEnteredTime' is not sent, the default `fromEnteredTime` would be 60 days from `toEnteredTime`.
        /// </param>
        /// <param name="orderStatusType">Specifies that only orders of this status should be returned.</param>
        public IEnumerable<OrderModel> GetOrdersByPath(int? maxResults = null, DateTime? fromEnteredTime = null, DateTime? toEnteredTime = null, OrderStatusType orderStatusType = OrderStatusType.NoValue)
        {
            var request = new RestRequest($"accounts/{_accountNumber}/orders", Method.GET);

            return GetOrderByDifferentPath(request, maxResults, fromEnteredTime, toEnteredTime, orderStatusType);
        }

        /// <summary>
        /// User Principal details
        /// </summary>
        public UserPrincipalsModel GetUserPrincipals()
        {
            var request = new RestRequest("userprincipals", Method.GET);

            request.AddQueryParameter("fields", "streamerSubscriptionKeys,streamerConnectionInfo,preferences,surrogateIds");

            return Execute<UserPrincipalsModel>(request);
        }

        #endregion

        #region POST

        /// <summary>
        /// Sign in using code from SignInUrl
        /// The token endpoint returns an access token along with an optional refresh token.
        /// https://developer.tdameritrade.com/authentication/apis/post/token-0
        /// </summary>
        /// <param name="code">Required if trying to use authorization code grant</param>
        /// <param name="redirectUrl">Required if trying to use authorization code grant</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AccessTokenModel PostAccessToken(GrantType grantType, string code)
        {
            var path = _restApiUrl + "oauth2/token";

            using (var client = new HttpClient())
            {
                var body = new Dictionary<string, string>();

                body["grant_type"] = grantType.GetEnumMemberValue();

                if (grantType == GrantType.RefreshToken)
                    body["refresh_token"] = _refreshToken;

                if (grantType == GrantType.AuthorizationCode)
                    body["access_type"] = "offline";

                if (grantType == GrantType.AuthorizationCode)
                    body["code"] = HttpUtility.UrlDecode(code);

                body["client_id"] = _consumerKey + "@AMER.OAUTHAP";

                if (grantType == GrantType.AuthorizationCode)
                    body["redirect_uri"] = "http://localhost";

                var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(body) };
                var res = client.Send(req);

                // mark start;
                var rest = new RestClient(_restApiUrl);
                var req2 = new RestRequest("oauth2/token", Method.POST);                
                foreach (var kv in body) {
                    req2.AddParameter(kv.Key, kv.Value, ParameterType.GetOrPost);
                }
                var res2 = rest.Execute(req2);
                // mark end;

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var accessTokens = JsonConvert.DeserializeObject<AccessTokenModel>(res.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        RestClient.AddOrUpdateDefaultParameter(new Parameter("Authorization", accessTokens.TokenType + " " + accessTokens.AccessToken, ParameterType.HttpHeader));
                        return accessTokens;
                    default:
                        Log.Error($"TDAmeritrade.SignIn: StatusCode:{res.StatusCode}, ReasonPhrase:{res.ReasonPhrase}");
                        throw new Exception($"{res.StatusCode} {res.ReasonPhrase}");
                }
            }
        }

        /// <summary>
        /// Place an order for a specific account.
        /// </summary>
        /// <param name="orderType">Market | Limit</param>
        /// <param name="sessionType">Market | Limit</param>
        /// <param name="durationType">Market | Limit</param>
        /// <param name="orderStrategyType">Market | Limit</param>
        /// <param name="orderLegCollectionModels">Market</param>
        /// <param name="complexOrderStrategyType">Limit</param>
        /// <param name="price">Limit</param>
        /// <param name="activationPrice">Stop Limit</param>
        /// <returns></returns>
        private string PostPlaceOrder(
            OrderType orderType,
            SessionType sessionType,
            DurationType durationType,
            OrderStrategyType orderStrategyType,
            List<PlaceOrderLegCollectionModel> orderLegCollectionModels,
            ComplexOrderStrategyType? complexOrderStrategyType = null,
            decimal price = 0m,
            decimal stopPrice = 0m)
        {
            var request = new RestRequest($"accounts/{_accountNumber}/orders", Method.POST);

            var body = new Dictionary<string, object>();

            if (orderType != OrderType.Market)
                body["complexOrderStrategyType"] = complexOrderStrategyType.GetEnumValue();

            if (orderType != OrderType.Market)
                body["price"] = price;

            body["orderType"] = orderType.GetEnumMemberValue();
            body["session"] = sessionType.GetEnumMemberValue();
            body["duration"] = durationType.GetEnumMemberValue();
            body["orderStrategyType"] = orderStrategyType.GetEnumMemberValue();
            body["orderLegCollection"] = orderLegCollectionModels;

            if(orderType == OrderType.StopLimit)
                body["stopPrice"] = stopPrice;

            request.AddJsonBody(JsonConvert.SerializeObject(body));

            return Execute<string>(request); // Place Order
        }

        public bool CancelOrder(string orderNumber, string? accountNumber = null)
        {
            var account = string.IsNullOrEmpty(accountNumber) ? _accountNumber : accountNumber;

            var request = new RestRequest($"accounts/{account}/orders/{orderNumber}", Method.DELETE);

            return string.IsNullOrEmpty(Execute<string>(request)) ? false : true;
        }

        #endregion


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

        private IEnumerable<OrderModel> GetOrderByDifferentPath(
            RestRequest request,
            int? maxResults = null,
            DateTime? fromEnteredTime = null,
            DateTime? toEnteredTime = null,
            OrderStatusType orderStatusType = OrderStatusType.NoValue)
        {
            if (maxResults.HasValue)
                request.AddQueryParameter("maxResults", maxResults.Value.ToStringInvariant());

            if (fromEnteredTime.HasValue && (toEnteredTime.HasValue ? toEnteredTime.Value > fromEnteredTime.Value : true))
                request.AddQueryParameter("fromEnteredTime", fromEnteredTime.Value.ToString("yyyy-MM-dd"));

            if (toEnteredTime.HasValue && (fromEnteredTime.HasValue ? fromEnteredTime.Value < toEnteredTime.Value : true))
                request.AddQueryParameter("toEnteredTime", toEnteredTime.Value.ToString("yyyy-MM-dd"));

            if (orderStatusType != OrderStatusType.NoValue)
                request.AddQueryParameter("status", orderStatusType.GetEnumValue());

            return Execute<List<OrderModel>>(request);
        }

        #endregion
    }
}
