using Newtonsoft.Json;
using QuantConnect.Logging;
using QuantConnect.TDAmeritrade.Domain.Enums;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels.MarketHours;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels.UserInfoAndPreferences;
using QuantConnect.TDAmeritrade.Utils.Extensions;
using RestSharp;
using System.Net;
using System.Web;

namespace QuantConnect.TDAmeritrade.Application
{
    public partial class TDAmeritrade
    {
        #region GET

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

            var jsonResponse = Execute(request);

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
        /// Account balances, positions, and orders for all linked accounts.
        /// </summary>
        public List<AccountModel> GetAccounts()
        {
            var request = new RestRequest("accounts", Method.GET);

            request.AddQueryParameter("fields", "positions,orders");
            
            return Execute<List<AccountModel>>(request);
        }

        /// <summary>
        /// Account balances, positions, and orders for a specific account.
        /// </summary>
        public List<AccountModel> GetAccount(string accountNumber)
        {
            var request = new RestRequest($"accounts/{accountNumber}", Method.GET);

            request.AddQueryParameter("fields", "positions,orders");

            return Execute<List<AccountModel>>(request);
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
        /// All orders for a specific account or, if account ID isn't specified, orders will be returned for all linked accounts.
        /// </summary>
        /// <param name="accountNumber">Account Number.</param>
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
        /// <returns></returns>
        public IEnumerable<OrderModel> GetOrdersByQuery(
            string accountNumber,
            int? maxResults = null, 
            DateTime? fromEnteredTime = null, 
            DateTime? toEnteredTime = null, 
            OrderStatusType orderStatusType = OrderStatusType.NoValue)
        {
            var request = new RestRequest($"orders", Method.GET);

            request.AddQueryParameter("accountId", accountNumber);

            return GetOrderByDifferentPath(request, maxResults, fromEnteredTime, toEnteredTime, orderStatusType);
        }

        /// <summary>
        /// Get a specific order for a specific account.
        /// </summary>
        /// <param name="orderNumber">Order Number</param>
        /// <param name="accountNumber">Account Number</param>
        /// <returns></returns>
        public OrderModel GetOrder(string orderNumber, string accountNumber = null)
        {
            var account = string.IsNullOrEmpty(accountNumber) ? _accountNumber : accountNumber;

            var request = new RestRequest($"accounts/{account}/orders/{orderNumber}", Method.GET);

            return Execute<OrderModel>(request);
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

        /// <summary>
        /// Retrieve market hours for specified markets
        /// </summary>
        public void GetHoursForMultipleMarkets()
        {
            var request = new RestRequest("marketdata/hours", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);



        }

        /// <summary>
        /// Retrieve market hours for specified single market
        /// </summary>
        /// <param name="marketType">EQUITY, OPTION, FUTURE, BOND, FOREX</param>
        /// <returns>return market data by specific market type</returns>
        public Dictionary<string, MarketHoursModel> GetHoursForSingleMarket(MarketType marketType)
        {
            var request = new RestRequest($"marketdata/{marketType.GetEnumValue()}/hours", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);

            var market = Execute<Dictionary<string, Dictionary<string, MarketHoursModel>>>(request);

            return market[market.Keys.First()];
        }

        /// <summary>
        /// Retrieve market hours for specified markets
        /// </summary>
        /// <param name="marketType">EQUITY, OPTION, FUTURE, BOND, FOREX</param>
        /// <returns>return market data by specific market type</returns>
        public Dictionary<string, Dictionary<string, MarketHoursModel>> GetHoursForMultipleMarkets(params MarketType[] marketTypes)
        {
            var request = new RestRequest("marketdata/hours", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);

            request.AddQueryParameter("markets", string.Join(',', marketTypes.Select(x => x.GetEnumValue())));

            var markets = Execute<Dictionary<string, Dictionary<string, MarketHoursModel>>>(request);

            return markets;
        }

        /// <summary>
        /// Top 10 (up or down) movers by value or percent for a particular market
        /// </summary>
        /// <param name="indexMoverType">The index symbol to get movers from. ($COMPX, $DJI, $SPX.X)</param>
        /// <param name="directionType">To return movers with the specified directions of up or down.</param>
        /// <param name="changeType">To return movers with the specified change types of percent or value</param>
        /// <returns></returns>
        public List<MoverModel> GetMovers(IndexMoverType indexMoverType, DirectionType directionType = DirectionType.NoValue, ChangeType changeType = ChangeType.NoValue)
        {
            var request = new RestRequest($"marketdata/{indexMoverType.GetEnumValue()}/movers", Method.GET);

            request.AddQueryParameter("apikey", _consumerKey);

            if(directionType != DirectionType.NoValue)
                request.AddQueryParameter("direction", directionType.GetEnumValue());

            if (changeType != ChangeType.NoValue)
                request.AddQueryParameter("change", changeType.GetEnumValue());

            return Execute<List<MoverModel>>(request);
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
        public async Task<AccessTokenModel> PostAccessToken(GrantType grantType, string code)
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
                    body["redirect_uri"] = _callbackUrl;

                var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(body) };
                var res = await client.SendAsync(req);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var accessTokens = JsonConvert.DeserializeObject<AccessTokenModel>(await res.Content.ReadAsStringAsync());
                        _accessToken = accessTokens.TokenType + " " + accessTokens.AccessToken;
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
        /// <returns></returns>
        public IEnumerable<OrderModel> PostPlaceOrder(
            OrderType orderType,
            SessionType sessionType, 
            DurationType durationType,
            OrderStrategyType orderStrategyType,
            List<PlaceOrderLegCollectionModel> orderLegCollectionModels,
            ComplexOrderStrategyType? complexOrderStrategyType = null,
            decimal price = 0m)
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

            request.AddJsonBody(JsonConvert.SerializeObject(body));

            Execute(request); // Place Order

            return GetOrdersByPath(orderLegCollectionModels.Count); // Get Order Detail
        }

        public bool CancelOrder(string orderNumber, string? accountNumber = null)
        {
            var account = string.IsNullOrEmpty(accountNumber) ? _accountNumber : accountNumber;

            var request = new RestRequest($"accounts/{account}/orders/{orderNumber}", Method.DELETE);

            return string.IsNullOrEmpty(Execute(request)) ? false : true;
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
            if(maxResults.HasValue)
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
