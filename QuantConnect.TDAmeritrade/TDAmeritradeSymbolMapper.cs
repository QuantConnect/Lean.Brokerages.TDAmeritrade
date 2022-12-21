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

using QuantConnect.Data.Auxiliary;
using QuantConnect.Interfaces;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public class TDAmeritradeSymbolMapper : ISymbolMapper
    {
        private readonly IMapFileProvider _mapFileProvider;

        // Generated map for websocket symbols, to have O(1) access
        private readonly Dictionary<string, Symbol> _wsSymbolMap = new();

        public TDAmeritradeSymbolMapper(IMapFileProvider mapFileProvider)
        {
            _mapFileProvider = mapFileProvider;
        }

        public string GetBrokerageSymbol(Symbol symbol)
        {
            if (symbol == null || string.IsNullOrWhiteSpace(symbol.Value))
            {
                throw new ArgumentException("TDAmeritrade:SymbolMapper:GetBrokerageSymbol(), Invalid symbol: " + (symbol == null ? "null" : symbol.ToString()));
            }

            if (symbol.ID.SecurityType != SecurityType.Equity)
            {
                throw new ArgumentException("TDAmeritrade:SymbolMapper:GetBrokerageSymbol(), Invalid security type: " + symbol.ID.SecurityType);
            }

            return GetMappedTicker(symbol);
        }

        public Symbol GetLeanSymbol(string brokerageSymbol, SecurityType securityType, string market, DateTime expirationDate = default, decimal strike = 0, OptionRight optionRight = OptionRight.Call)
        {
            var leanSymbolFormat = ConvertTDAmeritradeSymbolToLeanSymbol(brokerageSymbol);
            return Symbol.Create(leanSymbolFormat, securityType, Market.USA);
        }

        /// <summary>
        /// Get TD Ameritrade Websocket ticker for passed <see cref="Symbol"/>
        /// </summary>
        /// <param name="symbol">Lean <see cref="Symbol"/></param>
        /// <returns>Websocket symbol</returns>
        /// <exception cref="ArgumentException">Wrong Lean <see cref="Symbol"/></exception>
        public string GetBrokerageWebsocketSymbol(Symbol symbol)
        {
            var brokerageSymbol = GetBrokerageSymbol(symbol);

            brokerageSymbol = brokerageSymbol.Replace('.', '/').Replace('-', 'p').Replace("+", "/WS");

            _wsSymbolMap.TryAdd(brokerageSymbol, symbol);

            return brokerageSymbol;
        }

        /// <summary>
        /// Get Lean <see cref="Symbol"/> from TD Ameritrade Websocket</summary>
        /// <param name="brokerageWebSocketSymbol">brokerage symbol</param>
        /// <returns>Lean <see cref="Symbol"/>Lean symbol</returns>
        public Symbol GetLeanSymbolByBrokerageWebsocketSymbol(string brokerageWebSocketSymbol)
        {
            Symbol? leanSymbol;
            if (_wsSymbolMap.TryGetValue(brokerageWebSocketSymbol, out leanSymbol))
            {
                return leanSymbol;
            }

            throw new ArgumentException($"TDAmeritrade:SymbolMapper:GetSymbolFromWebsocket(), symbol hasn't kept in collection: {brokerageWebSocketSymbol}");
        }

        private string GetMappedTicker(Symbol symbol)
        {
            var ticker = symbol.ID.Symbol;
            if (symbol.ID.SecurityType == SecurityType.Equity)
            {
                var mapFile = _mapFileProvider.Get(AuxiliaryDataKey.Create(symbol)).ResolveMapFile(symbol);
                ticker = mapFile.GetMappedSymbol(DateTime.UtcNow, symbol.Value);
            }

            return ConvertLeanSymbolToTDAmeritradeSymbol(ticker);
        }

        /// <summary>
        /// Converts a Lean symbol string to an TDAmeritrade symbol
        /// </summary>
        private static string ConvertLeanSymbolToTDAmeritradeSymbol(string leanSymbol)
        {
            // Lean symbols are equal to TDAmeritrade symbols with dot instead dash
            return leanSymbol.Replace("-", ".");
        }

        /// <summary>
        /// Converts TDAmeritrade symbol string to a Lean symbol
        /// </summary>
        private static string ConvertTDAmeritradeSymbolToLeanSymbol(string brokerageSymbol)
        {
            // Lean symbols are equal to TDAmeritrade symbols with dot instead dash
            return brokerageSymbol.Replace(".", "-");
        }
    }
}
