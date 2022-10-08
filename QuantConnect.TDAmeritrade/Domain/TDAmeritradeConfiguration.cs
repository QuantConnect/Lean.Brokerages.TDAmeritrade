using QuantConnect.Configuration;

namespace QuantConnect.TDAmeritrade.Domain
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
    }
}
