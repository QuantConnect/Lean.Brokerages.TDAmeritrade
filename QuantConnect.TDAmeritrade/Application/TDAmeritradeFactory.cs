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
                    { "tdameritrade-consumer-key", TDAmeritradeConfiguration.ConsumerKey.ToStringInvariant() }
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

            var brokerage = new TDAmeritrade(consumerKey, algorithm);

            // Add the brokerage to the composer to ensure its accessible to the live data feed.
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

        public override void Dispose()
        { }

    }
}
