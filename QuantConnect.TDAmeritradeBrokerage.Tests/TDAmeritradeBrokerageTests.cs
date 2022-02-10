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

using NUnit.Framework;
using QuantConnect.Tests;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Tests.Brokerages;
using QuantConnect.Brokerages.TDAmeritrade;
using System;
using TDAmeritradeApi.Client;
using QuantConnect.Brokerages;
using QuantConnect.Tests.Common.Securities;
using Moq;
using QuantConnect.Util;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Tests.Engine;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using QuantConnect.Orders;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Tests.Engine.DataFeeds;
using QuantConnect.Algorithm;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Data.Market;
using QuantConnect.Packets;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Lean.Engine.HistoricalData;

namespace QuantConnect.TDAmeritradeDownloader.Tests
{
    [TestFixture]
    public partial class TDAmeritradeBrokerageTests : BrokerageTests
    {
        private bool arePaperTrading = true;
        protected override Symbol Symbol { get => Symbol.Create("SPY", SecurityType.Equity, Market.USA); }
        protected override SecurityType SecurityType { get => Symbol.SecurityType; }

        protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            var job = new LiveNodePacket() { BrokerageData = new()};

            job.BrokerageData.Add("live-cash-balance", "[{Amount:100000000, Currency=USD'}]");

            var marketHoursDatabase = MarketHoursDatabase.FromDataFolder();

            var exchangeHours = marketHoursDatabase.GetExchangeHours(Market.USA, Symbol, SecurityType.Equity);

            if (!exchangeHours.IsOpen(DateTime.Now, true))
                throw new NotSupportedException("Market is currently closed.");

            var synchronizer = new LiveSynchronizer();

            TestableLiveTradingDataFeed feed = new();

            var algorithm = new QCAlgorithm();

            var orderProcessor = new TestOrderProcessor(OrderProvider, algorithm.Transactions);
            algorithm.Transactions.SetOrderProcessor(orderProcessor);

            var tdBrokerage = new TDAmeritradeBrokerage(algorithm, algorithm.Transactions, algorithm.Portfolio, TDAmeritradeBrokerageFactory.Configuration.AccountID, tdCredentials: new DefaultTDCredentials(), paperTrade: arePaperTrading);
            tdBrokerage.Connect();
            tdBrokerage.SetJob(job);

            var symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();
            var mapFilePrimaryExchangeProvider = new MapFilePrimaryExchangeProvider(TestGlobals.MapFileProvider);
            var registeredTypesProvider = new RegisteredSecurityDataTypesProvider();
            var securityService = new SecurityService(algorithm.Portfolio.CashBook,
                marketHoursDatabase,
                symbolPropertiesDatabase,
                algorithm,
                registeredTypesProvider,
                new SecurityCacheProvider(algorithm.Portfolio),
                mapFilePrimaryExchangeProvider);

            algorithm.Securities.SetSecurityService(securityService);

            var dataManager = new DataManagerStub(feed, algorithm, 
                algorithm.TimeKeeper, marketHoursDatabase, securityService, true);

            algorithm.SubscriptionManager.SetDataManager(dataManager);

            synchronizer.Initialize(algorithm, dataManager);

            feed.DataQueueHandler = tdBrokerage;
            feed.Initialize(algorithm, job, new BacktestingResultHandler(),
                TestGlobals.MapFileProvider, TestGlobals.FactorFileProvider, TestGlobals.DataProvider, dataManager, synchronizer, new DataChannelProvider());

            var historyProvider = new BrokerageHistoryProvider();
            historyProvider.SetBrokerage(tdBrokerage);

            algorithm.HistoryProvider = historyProvider;

            //set up algorithm

            algorithm.SetLiveMode(true);
            algorithm.SetDateTime(DateTime.UtcNow);

            //Initialize
            algorithm.SetBrokerageModel(new TDAmeritradeBrokerageModel());

            var security = algorithm.AddSecurity(SecurityType.Equity, "SPY", Resolution.Minute, Market.USA, true, 1, true);

            SecurityProvider[Symbol] = security;

            var data = tdBrokerage.GetHistory(
                new Data.HistoryRequest(DateTime.UtcNow.AddMinutes(-30),
                DateTime.UtcNow, typeof(TradeBar), Symbol,
                Resolution.Minute, exchangeHours,
                TimeZones.NewYork, Resolution.Minute, true, false,
                DataNormalizationMode.Adjusted, TickType.Trade)).ToList();

            dataManager.Algorithm.Securities["SPY"].Update(data, data[0].GetType());

            dataManager.Algorithm.PostInitialize();

            DataQueueSubscribe(dataManager);

            return tdBrokerage;
        }

        private static void DataQueueSubscribe(DataManagerStub dataManager)
        {
            foreach (var subscribedSecurities in dataManager.Algorithm.Securities)
            {
                var security = subscribedSecurities.Value;
                foreach (var config in security.Subscriptions)
                {
                    var request = new SubscriptionRequest(false, null, security, config, DateTime.UtcNow.AddDays(-1), Time.EndOfTime);
                    dataManager.AddSubscription(request);
                }
            }
        }

        protected override bool IsAsync()
        {
            return false;
        }

        protected override decimal GetAskPrice(Symbol symbol)
        {
            if (Brokerage is TDAmeritradeBrokerage tdAmeritradeBrokerage)
            {
                var quote = tdAmeritradeBrokerage.GetMarketQuote(symbol);

                return quote.AskPrice;
            }
            else
                throw new NotSupportedException();
        }


        /// <summary>
        /// Provides the data required to test each order type in various cases
        /// </summary>
        private static TestCaseData[] OrderParameters()
        {
            return new[]
            {
                new TestCaseData(new MarketOrderTestParameters(Symbols.SPY)).SetName("MarketOrder"),
                new TestCaseData(new LimitOrderTestParameters(Symbols.SPY, 10000m, 0.01m)).SetName("LimitOrder"),
                new TestCaseData(new StopMarketOrderTestParameters(Symbols.SPY, 10000m, 0.01m)).SetName("StopMarketOrder"),
                new TestCaseData(new StopLimitOrderTestParameters(Symbols.SPY, 10000m, 0.01m)).SetName("StopLimitOrder"),
                new TestCaseData(new LimitIfTouchedOrderTestParameters(Symbols.SPY, 10000m, 0.01m)).SetName("LimitIfTouchedOrder")
            };
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void CancelOrders(OrderTestParameters parameters)
        {
            base.CancelOrders(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void LongFromZero(OrderTestParameters parameters)
        {
            base.LongFromZero(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void CloseFromLong(OrderTestParameters parameters)
        {
            base.CloseFromLong(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void ShortFromZero(OrderTestParameters parameters)
        {
            base.ShortFromZero(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void CloseFromShort(OrderTestParameters parameters)
        {
            base.CloseFromShort(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void ShortFromLong(OrderTestParameters parameters)
        {
            base.ShortFromLong(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void LongFromShort(OrderTestParameters parameters)
        {
            base.LongFromShort(parameters);
        }
    }

    internal class TestOrderProcessor : IOrderProcessor
    {
        private OrderProvider _orderProvider;
        private readonly SecurityTransactionManager _securityTransactionManager;

        public TestOrderProcessor(OrderProvider orderProvider, SecurityTransactionManager securityTransactionManager)
        {
            _orderProvider = orderProvider;
            _securityTransactionManager = securityTransactionManager;
        }

        public int OrdersCount => _orderProvider.OrdersCount;

        public List<Order> GetOpenOrders(Func<Order, bool> filter = null)
        {
            return _orderProvider.GetOpenOrders(filter);
        }

        public IEnumerable<OrderTicket> GetOpenOrderTickets(Func<OrderTicket, bool> filter = null)
        {
            return _orderProvider.GetOpenOrderTickets(filter);
        }

        public Order GetOrderByBrokerageId(string brokerageId)
        {
            return _orderProvider.GetOrderByBrokerageId(brokerageId);
        }

        public Order GetOrderById(int orderId)
        {
            return _orderProvider.GetOrderById(orderId);
        }

        public IEnumerable<Order> GetOrders(Func<Order, bool> filter = null)
        {
            return _orderProvider.GetOrders(filter);
        }

        public OrderTicket GetOrderTicket(int orderId)
        {
            return _orderProvider.GetOrderById(orderId)?.ToOrderTicket(_securityTransactionManager);
        }

        public IEnumerable<OrderTicket> GetOrderTickets(Func<OrderTicket, bool> filter = null)
        {
            return _orderProvider.GetOrders(Convert(filter)).Select(order => order.ToOrderTicket(_securityTransactionManager));
        }

        private Func<Order, bool> Convert(Func<OrderTicket, bool> filter)
        {
            return order => filter?.Invoke(order.ToOrderTicket(_securityTransactionManager)) ?? true;
        }

        public OrderTicket Process(OrderRequest request)
        {
            return null;
        }
    }
}