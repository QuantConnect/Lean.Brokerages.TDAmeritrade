using QuantConnect.Configuration;

namespace QuantConnect.TDAmeritrade.Domain
{
    public static class TDAmeritradeConfiguration
    {
        /// <summary>
        /// Gets whether to use the developer sandbox or not
        /// </summary>
        public static string ConsumerKey => Config.Get("tdameritrade-consumer-key");
    }
}
