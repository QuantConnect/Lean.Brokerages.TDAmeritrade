using Newtonsoft.Json;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Packets;
using QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.TDAmeritrade.Application
{
    public partial class TDAmeritrade
    {
        private SemaphoreSlim _slim = new SemaphoreSlim(1);

        /// <summary>
        /// Returns true if we're currently connected to the broker
        /// </summary>
        public override bool IsConnected => WebSocket.IsOpen;

        public void SetJob(LiveNodePacket job)
        {
            if (!IsConnected)
            {
                Connect();
            }
        }

        public override void Disconnect()
        {
            if (WebSocket != null && WebSocket.IsOpen)
            {
                WebSocket.Close();
            }
        }

        protected override bool Subscribe(IEnumerable<Symbol> symbols)
        {
            var symbolsAdded = false;

            foreach (var symbol in symbols)
            {
                if (!symbol.IsCanonical())
                {
                    //var ticker = _symbolMapper.GetBrokerageSymbol(symbol);
                    //if (!_subscribedTickers.ContainsKey(ticker))
                    //{
                    //    _subscribedTickers.TryAdd(ticker, symbol);
                    //    symbolsAdded = true;
                    //}
                    symbolsAdded = true;
                }
            }

            if (symbolsAdded)
            {
                //SendSubscribeMessage(_subscribedTickers.Keys.ToList());
                if (WebSocket.IsOpen)
                {
                    WebSocket.Send(Login());
                }
            }

            return true;
        }

        public IEnumerator<BaseData> Subscribe(SubscriptionDataConfig dataConfig, EventHandler newDataAvailableHandler)
        {
            //if (!CanSubscribe(dataConfig.Symbol))
            //{
            //    return Enumerable.Empty<BaseData>().GetEnumerator();
            //}

            var enumerator = _aggregator.Add(dataConfig, newDataAvailableHandler);
            SubscriptionManager.Subscribe(dataConfig);

            return enumerator;
        }

        private bool Unsubscribe(IEnumerable<Symbol> symbols)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(SubscriptionDataConfig dataConfig)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles websocket received messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void OnMessage(object sender, WebSocketMessage e)
        {
            throw new NotImplementedException();
        }

        public string Login()
        {
            var userPrincipals = GetUserPrincipals();

            var tokenTimeStampAsDateObj = DateTime.Parse(userPrincipals.StreamerInfo.TokenTimestamp);
            var tokenTimeStampAsMs = Time.DateTimeToUnixTimeStampMilliseconds(tokenTimeStampAsDateObj);

            var credentials = new СredentialsModel(
                userPrincipals.Accounts[0].AccountId,
                userPrincipals.StreamerInfo.Token,
                userPrincipals.Accounts[0].Company,
                userPrincipals.Accounts[0].Segment,
                userPrincipals.Accounts[0].AccountCdDomainId,
                userPrincipals.StreamerInfo.UserGroup,
                userPrincipals.StreamerInfo.AccessLevel,
                tokenTimeStampAsMs,
                userPrincipals.StreamerInfo.AppId,
                userPrincipals.StreamerInfo.Acl);

            var request = new StreamRequestModelContainer
            {
                Requests = new StreamRequestModel[]
                {
                    new StreamRequestModel
                    {
                        Service = "ADMIN",
                        Command = "LOGIN",
                        Requestid = 0, // Interlocked.Increment(ref _counter),
                        Account = userPrincipals.Accounts[0].AccountId,
                        Source = userPrincipals.StreamerInfo.AppId,
                        Parameters = new
                        {
                            token = userPrincipals.StreamerInfo.Token,
                            version = "1.0",
                            credential = JsonConvert.SerializeObject(credentials),
                        }
                    }
                }
            };

            return JsonConvert.SerializeObject(request);
        }
    }
}
