using QuantConnect.Brokerages.TDAmeritrade.Models;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public class TDAmeritradeBrokerageFactory : BrokerageFactory
    {
        public override Dictionary<string, string> BrokerageData
        {
            get
            {
                var data = new Dictionary<string, string>()
                {
                    { "tdameritrade-consumer-key", TDAmeritradeConfiguration.ConsumerKey.ToStringInvariant() },
                    { "tdameritrade-callback-url", TDAmeritradeConfiguration.CallbackUrl.ToStringInvariant() },
                    { "tdameritrade-code-from-url", TDAmeritradeConfiguration.AccessToken.ToStringInvariant() },
                    { "tdameritrade-refresh-token", TDAmeritradeConfiguration.RefreshToken.ToStringInvariant() },
                    { "tdameritrade-account-number", TDAmeritradeConfiguration.AccountNumber.ToStringInvariant() }
                };
                return data;
            }
        }

        public TDAmeritradeBrokerageFactory() : base(typeof(TDAmeritradeBrokerage))
        { }

        public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new TradierBrokerageModel();

        public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
        {
            var errors = new List<string>();

            var consumerKey = Read<string>(job.BrokerageData, "tdameritrade-consumer-key", errors);
            var callback = Read<string>(job.BrokerageData, "tdameritrade-callback-url", errors);
            var codeFromUrl = Read<string>(job.BrokerageData, "tdameritrade-code-from-url", errors);
            var refreshToken = Read<string>(job.BrokerageData, "tdameritrade-refresh-token", errors);
            var accountNumber = Read<string>(job.BrokerageData, "tdameritrade-account-number", errors);

            var brokerage = new TDAmeritradeBrokerage(consumerKey, refreshToken, callback, codeFromUrl, accountNumber, algorithm, algorithm.Portfolio);

            // Add the brokerage to the composer to ensure its accessible to the live data feed.
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

        public override void Dispose()
        { }
    }
}
