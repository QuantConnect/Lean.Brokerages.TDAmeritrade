using QuantConnect.Brokerages;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.TDAmeritrade.Domain;
using QuantConnect.Util;

namespace QuantConnect.TDAmeritrade.Application
{
    public class TDAmeritradeFactory : BrokerageFactory
    {

        public override Dictionary<string, string> BrokerageData
        {
            get
            {
                var data = new Dictionary<string, string>()
                {
                    { "tdameritrade-consumer-key", TDAmeritradeConfiguration.ConsumerKey.ToStringInvariant() },
                    { "tdameritrade-access-token", TDAmeritradeConfiguration.AccessToken.ToStringInvariant() },
                    { "tdameritrade-refresh-token", TDAmeritradeConfiguration.RefreshToken.ToStringInvariant() }
                };
                return data;
            }
        }

        public TDAmeritradeFactory() : base(typeof(TDAmeritrade))
        { }

        public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new TradierBrokerageModel();

        public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
        {
            var errors = new List<string>();

            var consumerKey = Read<string>(job.BrokerageData, "tdameritrade-consumer-key", errors);
            var accessToken = Read<string>(job.BrokerageData, "tdameritrade-access-token", errors);
            var refreshToken = Read<string>(job.BrokerageData, "tdameritrade-refresh-token", errors);

            var brokerage = new TDAmeritrade(consumerKey, accessToken, refreshToken, algorithm);

            // Add the brokerage to the composer to ensure its accessible to the live data feed.
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

        public override void Dispose()
        { }

    }
}
