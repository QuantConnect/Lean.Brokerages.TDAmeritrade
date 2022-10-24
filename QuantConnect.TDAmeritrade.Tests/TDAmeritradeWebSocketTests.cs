using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public partial class TDAmeritradeTests
    {
        [Test]
        public void StreamsData()
        {
            var cancelationToken = new CancellationTokenSource();
            var brokerage = (TDAmeritradeBrokerage)Brokerage;

            var symbol = Symbols.AAPL;
            var symbol2 = Symbols.IBM;

            var configs = new[] {
                GetSubscriptionDataConfig<QuoteBar>(symbol, Resolution.Tick),
                GetSubscriptionDataConfig<TradeBar>(symbol2, Resolution.Tick)
                };

            foreach (var config in configs)
            {
                ProcessFeed(brokerage.Subscribe(config, (s, e) => { }),
                    cancelationToken,
                    (baseData) =>
                    {
                        if (baseData != null) { Log.Trace($"{baseData}"); }
                    });
            }

            Thread.Sleep(10000);

            foreach (var config in configs)
                brokerage.Unsubscribe(config);

            Thread.Sleep(5000);

            cancelationToken.Cancel();
        }
    }
}
