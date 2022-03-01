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

using QuantConnect.Interfaces;
using QuantConnect.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDAmeritradeApi.Client.Models.MarketData;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    /// <summary>
    /// TDAmeritrade Class: IOptionChainProvider implementation
    /// </summary>
    public partial class TDAmeritradeBrokerage : IOptionChainProvider
    {
        /// <summary>
        /// Gets the list of option contracts for a given underlying symbol
        /// </summary>
        /// <param name="symbol">The underlying symbol</param>
        /// <param name="date">The date for which to request the option chain (only used in backtesting)</param>
        /// <returns>The list of option contracts</returns>
        public IEnumerable<Symbol> GetOptionContractList(Symbol symbol, DateTime date)
        {
            try
            {
                if (_nonOrderRateGate.IsRateLimited)
                {
                    _nonOrderRateGate.WaitToProceed();
                }

                var optionsChain = _tdAmeritradeClient.MarketDataApi.GetOptionChainAsync(symbol.Value).Result;

                List<Symbol> options = new List<Symbol>();

                options.AddRange(CreateSymbols(symbol, optionsChain.callExpDateMap, OptionRight.Call));
                options.AddRange(CreateSymbols(symbol, optionsChain.putExpDateMap, OptionRight.Put));

                return options;
            }
            catch { }

            return Enumerable.Empty<Symbol>(); ;
        }

        /// <returns></returns>
        /// <summary>
        /// Gets the list of option contracts for a given underlying symbol
        /// </summary>
        /// <param name="symbol">The underlying symbol</param>
        /// <param name="optionChain">strike price to expiration lookup</param>
        /// <param name="optionRight">call or put</param>
        /// <returns>The list of option contracts</returns>
        private static List<Symbol> CreateSymbols(Symbol symbol, Dictionary<string, Dictionary<decimal, List<ExpirationDateMap>>> optionChain, OptionRight optionRight)
        {
            List<Symbol> options = new List<Symbol>();
            foreach (var option in optionChain)
            {
                var dateAndDaysToExpiration = option.Key.Split(':');
                var strikes = option.Value.Keys.ToList();

                foreach (var strike in strikes)
                {
                    options.Add(Symbol.CreateOption(symbol, Market.USA.ToString(), OptionStyle.American, optionRight, strike, DateTime.Parse(dateAndDaysToExpiration[0], CultureInfo.InvariantCulture)));
                }
            }
            return options;
        }
    }
}