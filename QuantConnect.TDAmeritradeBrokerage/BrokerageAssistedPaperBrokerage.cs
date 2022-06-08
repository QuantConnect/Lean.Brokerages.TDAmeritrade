using Newtonsoft.Json;
using QuantConnect.Brokerages.Paper;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public class BrokerageAssistedPaperBrokerage : Brokerage
    {
        private readonly PaperBrokerage _paperBrokerage;
        private readonly IAlgorithm _algorithm;
        private readonly Brokerage _brokerage;

        public BrokerageAssistedPaperBrokerage(IAlgorithm algorithm, Brokerage brokerage, LiveNodePacket job, Func<List<CashAmount>> getBrokerageCashBalance = null)
            : base("Brokerage-Assisted Paper Brokerage")
        {
            if(!job.BrokerageData.ContainsKey("live-cash-balance"))
            {
                job.BrokerageData.Add("live-cash-balance",
                    getBrokerageCashBalance != null ?
                    ConvertToJson(getBrokerageCashBalance()) : "[{Amount:100000000, Currency=USD'}]");
            }

            _paperBrokerage = new PaperBrokerage(algorithm, job);
            _algorithm = algorithm;
            _brokerage = brokerage;
            _paperBrokerage.OrderStatusChanged += (sender, e) => OnOrderEvent(e);
        }

        private static string ConvertToJson(List<CashAmount> cashAmounts)
        {
            var result = JsonConvert.SerializeObject(cashAmounts);

            return result;
        }

        public override bool IsConnected => _brokerage.IsConnected;

        public override bool CancelOrder(Order order)
        {
            return _paperBrokerage.CancelOrder(order);
        }

        public override void Connect()
        {
            _brokerage.Connect();
        }

        public override void Disconnect()
        {
            _brokerage.Disconnect();
        }

        public override List<Holding> GetAccountHoldings()
        {
            return _paperBrokerage.GetAccountHoldings();
        }

        public override List<CashAmount> GetCashBalance()
        {
            var paper = _paperBrokerage.GetCashBalance();

            return paper;
        }

        public override List<Order> GetOpenOrders()
        {
            return _paperBrokerage.GetOpenOrders();
        }

        public override bool PlaceOrder(Order order)
        {
            IfSecurityDoesntExistAddIt(order);

            order.Status = OrderStatus.New;
            return _paperBrokerage.PlaceOrder(order);
        }

        private void IfSecurityDoesntExistAddIt(Order order)
        {
            if (((ISecurityProvider)_algorithm.Portfolio).GetSecurity(order.Symbol) is null)
            {
                var security = AddSecurity(order);
                if (security != null)
                {
                    var time = DateTime.UtcNow;
                    var history = _brokerage.GetHistory(
                        new Data.HistoryRequest(time.AddMinutes(-10),
                        time, typeof(TradeBar), order.Symbol, Resolution.Minute,
                        null, null, null, false, false,
                        DataNormalizationMode.Adjusted, TickType.Trade)).ToList();

                    security.Update(history, typeof(TradeBar));
                }
            }
        }

        private Security AddSecurity(Order order)
        {
            return _algorithm.AddSecurity(order.Symbol.SecurityType, order.Symbol.Value, Resolution.Minute, Market.USA, true, 1, false);
        }

        public override bool UpdateOrder(Order order)
        {
            return _paperBrokerage.UpdateOrder(order);
        }

        public override void Dispose()
        {
            _paperBrokerage.OrderStatusChanged -= (sender, e) => OnOrderEvent(e);
            _paperBrokerage.DisposeSafely();
        }

        internal void TryAndFillOrders()
        {
            _paperBrokerage.Scan();
        }
    }
}
