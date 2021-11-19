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
 *
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuantConnect.Brokerages.Paper;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.TimeInForces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using QuantConnect.Util;
using RestSharp;
using TDAmeritradeApi.Client;
using TDAmeritradeApi.Client.Models;
using TDAmeritradeApi.Client.Models.AccountsAndTrading;
using TDAmeritradeApi.Client.Models.MarketData;
using AccountsAndTrading = TDAmeritradeApi.Client.Models.AccountsAndTrading;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    /// <summary>
    /// TDAmeritradeBrokerage Class:
    ///  - Handle authentication.
    ///  - Data requests.
    ///  - Rate limiting.
    ///  - Placing orders.
    ///  - Getting user data.
    /// </summary>
    [BrokerageFactory(typeof(TDAmeritradeBrokerageFactory))]
    public partial class TDAmeritradeBrokerage : Brokerage, IDataQueueHandler, IDataQueueUniverseProvider, IHistoryProvider, IOptionChainProvider
    {
        private string _accountId;
        private static readonly object _apiClientLock = new();
        private static readonly RateGate _nonOrderRateGate = new(1, TimeSpan.FromSeconds(1)); //  personal use non-commercial applications

        // we're reusing the equity exchange here to grab typical exchange hours
        private static readonly EquityExchange Exchange =
            new EquityExchange(MarketHoursDatabase.FromDataFolder().GetExchangeHours(Market.USA, null, SecurityType.Equity));

        private readonly SymbolPropertiesDatabase _symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();

        // polling timer for checking for fill events
        private readonly Timer _orderFillTimer;
        private readonly IAlgorithm _algorithm;
        private readonly IOrderProvider _orderProvider;
        private readonly ISecurityProvider _securityProvider;
        private readonly IDataAggregator _aggregator;

        private readonly EventBasedDataQueueHandlerSubscriptionManager _subscriptionManager;

        /// <summary>
        /// Returns the brokerage account's base currency
        /// </summary>
        public override string AccountBaseCurrency => Currencies.USD;

        private readonly HashSet<string> ErrorsDuringMarketHours = new HashSet<string>
        {
            "CheckForFillsError", "UnknownIdResolution", "ContingentOrderError", "NullResponse", "PendingOrderNotReturned"
        };

        private BrokerageAssistedPaperBrokerage _paperBrokerage;
        private readonly bool _paperTrade;
        private readonly TDAmeritradeClient _tdClient;

        public bool IsPaperTrading { get => _paperTrade && _paperBrokerage != null; }

        /// <summary>
        /// Create a new TDAmeritradeBrokerage Object:
        /// </summary>
        public TDAmeritradeBrokerage(
            IAlgorithm algorithm,
            IOrderProvider orderProvider,
            ISecurityProvider securityProvider,
            string accountId = null,
            string clientId = null,
            string redirectUri = null,
            ICredentials tdCredentials = null,
            bool paperTrade = false)
            : base("TD Ameritrade Brokerage")
        {
            _algorithm = algorithm;
            _paperTrade = paperTrade;
            _orderProvider = orderProvider;
            _securityProvider = securityProvider;
            _aggregator = Composer.Instance.GetExportedValueByTypeName<IDataAggregator>(Config.Get("data-aggregator", "QuantConnect.Lean.Engine.DataFeeds.AggregationManager"));

            _subscriptionManager = new EventBasedDataQueueHandlerSubscriptionManager();
            _subscriptionManager.SubscribeImpl += Subscribe;
            _subscriptionManager.UnsubscribeImpl += Unsubscribe;
            _tdClient = InitializeClient(clientId, redirectUri, tdCredentials);

            DetermineOrValidateAccount(accountId).Wait();
        }

        /// <summary>
        /// Determine if there is only 1 account and if so set the account ID else
        /// validate that specified account actually exists.
        /// </summary>
        /// <param name="accountId">specified account ID can be null, empty, or whitespace</param>
        /// <returns></returns>
        private async Task DetermineOrValidateAccount(string accountId)
        {
            _accountId = accountId;

            if (string.IsNullOrWhiteSpace(_accountId))
            {
                Log.Trace("No TD Ameritrade account specified. Checking if only 1 exists");

                var accounts = await _tdClient.AccountsAndTradingApi.GetAllAccountsAsync();

                accounts.DoForEach(account =>
                {
                    Log.Trace($"Found account: ID: {account.accountId} Type: {Enum.GetName(account.type)} IsDayTrader: {account.isDayTrader} RoundTrips: {account.roundTrips}");

                    if (account is CashAccount cashAccount)
                    {
                        Log.Trace($"\t Current Trading Balance {cashAccount.currentBalances.cashAvailableForTrading}");
                    }
                    else if (account is MarginAccount marginAccount)
                    {
                        Log.Trace($"\t Available Funds (Non-Margin) {marginAccount.currentBalances.availableFundsNonMarginableTrade}");
                    }
                });

                if (accounts.Count == 1)
                {
                    _accountId = accounts[0].accountId;
                    Log.Trace("Setting account ID to the only account.");
                }
                else
                {
                    Log.Error("There are multiple accounts. Add account ID to config file.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    await _tdClient.AccountsAndTradingApi.GetAccountAsync(_accountId);
                }
                catch
                {
                    Log.Error($"Account {_accountId} does not exist.");
                    Environment.Exit(1);
                }
            }
        }

        /// <summary>
        /// Initialize TDAmeritrade Api client.
        /// All information can be acquired by creating a new app at https://developer.tdameritrade.com/user/me/apps
        /// </summary>
        /// <param name="clientId">This is the consumer key that is generated in MyApps</param>
        /// <param name="redirectUri">This is the callback url that is defined in MyApps</param>
        /// <param name="tdCredentials">Callback interface for supplying username, password, and multi-factor authorization code</param>
        public static TDAmeritradeClient InitializeClient(string clientId = null, string redirectUri = null, ICredentials tdCredentials = null)
        {
            lock (_apiClientLock)
            {
                if(_nonOrderRateGate.IsRateLimited)
                {
                    _nonOrderRateGate.WaitToProceed();
                }

                if (tdCredentials is null)
                    tdCredentials = TDAmeritradeBrokerageFactory.Configuration.Credentials;
                if (clientId is null)
                    clientId = TDAmeritradeBrokerageFactory.Configuration.ConsumerKey;
                if (redirectUri is null)
                    redirectUri = TDAmeritradeBrokerageFactory.Configuration.CallbackUrl;

                var tdAmeritradeClient = new TDAmeritradeClient(clientId, redirectUri);
                Thread.Sleep(2000); //wait 2 seconds regardless of rate gate
                tdAmeritradeClient.LogIn(tdCredentials).Wait();

                return tdAmeritradeClient;
            }
        }

        #region IBrokerage implementation

        /// <summary>
        /// Returns true if we're currently connected to the broker
        /// </summary>
        public override bool IsConnected => _tdClient.LiveMarketDataStreamer.IsConnected;

        /// <summary>
        /// Gets all open orders on the account.
        /// </summary>
        /// <returns>The open orders returned from IB</returns>
        public override List<Order> GetOpenOrders()
        {
            if (IsPaperTrading)
            {
                return _paperBrokerage.GetOpenOrders();
            }

            var orders = new List<Order>();

            var openOrders = _tdClient.AccountsAndTradingApi.GetAllOrdersAsync(_accountId, OrderStrategyStatusType.QUEUED).Result;

            foreach (var openOrder in openOrders)
            {
                var order = TDAmeritradeToLeanMapper.ConvertOrder(openOrder);
                orders.Add(order);
            }

            return orders;
        }

        /// <summary>
        /// Gets all holdings for the account
        /// </summary>
        /// <returns>The current holdings from the account</returns>
        public override List<Holding> GetAccountHoldings()
        {
            if (IsPaperTrading)
            {
                return _paperBrokerage.GetAccountHoldings();
            }

            var holdings = GetPositions().Select(ConvertToHolding).Where(x => x.Quantity != 0).ToList();
            var tickers = holdings.Select(x => TDAmeritradeToLeanMapper.GetBrokerageSymbol(x.Symbol)).ToList();

            var quotes = GetQuotes(tickers);
            foreach (var holding in holdings)
            {
                var ticker = TDAmeritradeToLeanMapper.GetBrokerageSymbol(holding.Symbol);

                if (quotes.TryGetValue(ticker, out MarketQuote quote))
                {
                    holding.MarketPrice = quote.LastPrice;
                }
            }
            return holdings;
        }

        public MarketQuote GetMarketQuote(Symbol symbol)
        {
            if(_nonOrderRateGate.IsRateLimited)
            {
                _nonOrderRateGate.WaitToProceed();
            }

            var brokerSymbol = TDAmeritradeToLeanMapper.GetBrokerageSymbol(symbol);

            return _tdClient.MarketDataApi.GetQuote(brokerSymbol).Result;
        }

        /// <summary>
        /// Get Quotes wrapper of api
        /// </summary>
        /// <param name="tickers">ticker symbols to get quotes of</param>
        /// <returns>a dictionary mapping ticker symbol to quote</returns>
        private Dictionary<string, MarketQuote> GetQuotes(List<string> tickers)
        {
            return _tdClient.MarketDataApi.GetQuotes(tickers.ToArray()).Result;
        }

        /// <summary>
        /// Convert api position object to LEAN holding object
        /// </summary>
        /// <param name="position">trading position</param>
        /// <returns>LEAN holding object</returns>
        private Holding ConvertToHolding(Position position)
        {
            var symbol = TDAmeritradeToLeanMapper.GetSymbolFrom(position.instrument);

            var averagePrice = position.averagePrice;
            if (symbol.SecurityType == SecurityType.Option)
            {
                var multiplier = _symbolPropertiesDatabase.GetSymbolProperties(
                        symbol.ID.Market,
                        symbol,
                        symbol.SecurityType,
                        AccountBaseCurrency)
                    .ContractMultiplier;

                averagePrice /= multiplier;
            }

            return new Holding
            {
                Symbol = symbol,
                AveragePrice = averagePrice,
                CurrencySymbol = "$",
                MarketPrice = 0m, //--> GetAccountHoldings does a call to GetQuotes to fill this data in
                Quantity = position.shortQuantity == 0 ? position.longQuantity : position.shortQuantity
            };
        }

        /// <summary>
        /// Gets current holdings from TD Ameritrade
        /// </summary>
        /// <returns>positions</returns>
        private IEnumerable<AccountsAndTrading.Position> GetPositions()
        {
            var account = _tdClient.AccountsAndTradingApi.GetAccountAsync(_accountId).Result;

            return account.positions ?? Enumerable.Empty<AccountsAndTrading.Position>();
        }

        /// <summary>
        /// Gets the current cash balance for each currency held in the brokerage account
        /// </summary>
        /// <returns>The current cash balance for each currency available for trading</returns>
        public override List<CashAmount> GetCashBalance()
        {
            return new List<CashAmount>
            {
                new CashAmount(GetCurrentCashBalance(), AccountBaseCurrency)
            };
        }

        /// <summary>
        /// Get current cash balance from TD Ameritrade
        /// </summary>
        /// <returns>cash balance</returns>
        private decimal GetCurrentCashBalance()
        {
            var account = _tdClient.AccountsAndTradingApi.GetAccountAsync(_accountId).Result;

            if (account is CashAccount cashAccount)
                return cashAccount.currentBalances.totalCash;
            else if (account is MarginAccount marginAccount)
                return marginAccount.currentBalances.cashBalance;
            else
                return 0;
        }

        /// <summary>
        /// Places a new order and assigns a new broker ID to the order
        /// </summary>
        /// <param name="order">The order to be placed</param>
        /// <returns>True if the request for a new order has been placed, false otherwise</returns>
        public override bool PlaceOrder(Order order)
        {
            try
            {
                Log.Trace($"{nameof(TDAmeritradeBrokerage)}.PlaceOrder(): {order}");

                var holdingQuantity = _securityProvider.GetHoldingsQuantity(order.Symbol);
                var orderStrategy = TDAmeritradeToLeanMapper.ConvertToOrderStrategy(order, holdingQuantity);

                if (IsPaperTrading)
                {
                    _paperBrokerage.PlaceOrder(order);
                }
                else
                {
                    _tdClient.AccountsAndTradingApi.PlaceOrderAsync(_accountId, orderStrategy).Wait();
                }

                order.Status = OrderStatus.Submitted;
            }
            catch (Exception ex)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "PlaceOrderError", ex.Message));

                order.Status = OrderStatus.Invalid;
            }

            OnOrderEvent(new OrderEvent(order, order.Time, OrderFee.Zero));

            return order.Status == OrderStatus.Submitted;
        }

        /// <summary>
        /// Updates the order with the same id
        /// </summary>
        /// <param name="order">The new order information</param>
        /// <returns>True if the request was made for the order to be updated, false otherwise</returns>
        public override bool UpdateOrder(Order order)
        {
            Log.Trace($"{nameof(TDAmeritradeBrokerage)}.UpdateOrder(): {order}");

            try
            {
                var replaceOrder = TDAmeritradeToLeanMapper.ConvertToOrderStrategy(order, order.Quantity);

                if (IsPaperTrading)
                {
                    _paperBrokerage.UpdateOrder(order);
                }
                else
                {
                    _tdClient.AccountsAndTradingApi.ReplaceOrderAsync(_accountId, long.Parse(order.BrokerId.First(), CultureInfo.InvariantCulture), replaceOrder).Wait();
                }
                return true;
            }
            catch (Exception ex)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "UpdateOrderError", ex.Message));

                return false;
            }
        }

        /// <summary>
        /// Cancels the order with the specified ID
        /// </summary>
        /// <param name="order">The order to cancel</param>
        /// <returns>True if the request was made for the order to be canceled, false otherwise</returns>
        public override bool CancelOrder(Order order)
        {
            Log.Trace($"{nameof(TDAmeritradeBrokerage)}.CancelOrder(): {order}");

            try
            {
                if (IsPaperTrading)
                {
                    if (_paperBrokerage.CancelOrder(order))
                    {
                        var cancelEvent = new OrderEvent(order, order.Time, OrderFee.Zero);
                        cancelEvent.Status = OrderStatus.Canceled;
                        OnOrderEvent(cancelEvent);
                    }
                }
                else
                {
                    _tdClient.AccountsAndTradingApi.CancelOrderAsync(_accountId, long.Parse(order.BrokerId.First(), CultureInfo.InvariantCulture)).Wait();
                }

                return true;
            }
            catch (Exception ex)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "CancelOrderError", ex.Message));

                return false;
            }
        }

        /// <summary>
        /// Connects the client to the broker's remote servers
        /// </summary>
        public override void Connect()
        {
            lock (_apiClientLock)
            {
                _tdClient.LiveMarketDataStreamer.LoginAsync(_accountId).Wait();
            }
        }

        /// <summary>
        /// Disconnects the client from the broker's remote servers
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                _tdClient.LiveMarketDataStreamer.LogoutAsync().Wait();
            }
            catch { }
        }

        /// <summary>
        /// Dispose of the brokerage instance
        /// </summary>
        public override void Dispose()
        {
            _orderFillTimer.DisposeSafely();
            if (IsPaperTrading)
            {
                _paperBrokerage.OrderStatusChanged -= (sender, e) => OnOrderEvent(e);
                _paperBrokerage.DisposeSafely();
            }
        }

        /// <summary>
        /// Event invocator for the Message event
        /// </summary>
        /// <param name="e">The error</param>
        protected override void OnMessage(BrokerageMessageEvent e)
        {
            var message = e;
            if (Exchange.DateTimeIsOpen(DateTime.Now) && ErrorsDuringMarketHours.Contains(e.Code))
            {
                // elevate this to an error
                message = new BrokerageMessageEvent(BrokerageMessageType.Error, e.Code, e.Message);
            }
            base.OnMessage(message);
        }
        #endregion
    }
}
