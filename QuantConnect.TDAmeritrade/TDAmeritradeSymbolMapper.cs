using QuantConnect.Brokerages;

namespace QuantConnect.Brokerages.TDAmeritrade
{
    public class TDAmeritradeSymbolMapper : ISymbolMapper
    {
        // Generated map for websocket symbols, to have O(1) access
        private readonly Dictionary<string, Symbol> _wsSymbolMap = new();

        public string GetBrokerageSymbol(Symbol symbol)
        {
            if (symbol == null || string.IsNullOrWhiteSpace(symbol.Value))
                throw new ArgumentException("TDAmeritrade:SymbolMapper:GetBrokerageSymbol(), Invalid symbol: " + (symbol == null ? "null" : symbol.ToString()));

            if (symbol.ID.SecurityType != SecurityType.Equity 
                && symbol.ID.SecurityType != SecurityType.Option
                && symbol.ID.SecurityType != SecurityType.Index
                && symbol.ID.SecurityType != SecurityType.IndexOption)
                throw new ArgumentException("TDAmeritrade:SymbolMapper:GetBrokerageSymbol(), Invalid security type: " + symbol.ID.SecurityType);

            return symbol.Value;
        }

        public Symbol GetLeanSymbol(string brokerageSymbol, SecurityType securityType, string market, DateTime expirationDate = default, decimal strike = 0, OptionRight optionRight = OptionRight.Call)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get TD Ameritrade Websocket ticker for passed <see cref="Symbol"/>
        /// </summary>
        /// <param name="symbol">Lean <see cref="Symbol"/></param>
        /// <returns>Websocket symbol</returns>
        /// <exception cref="ArgumentException">Wrong Lean <see cref="Symbol"/></exception>
        public string GetWebsocketSymbol(Symbol symbol)
        {
            _wsSymbolMap.TryAdd(symbol.Value, symbol);

            return symbol.Value;
        }

        /// <summary>
        /// Get Lean <see cref="Symbol"/> from TD Ameritrade Websocket</summary>
        /// <param name="wsSymbol"></param>
        /// <returns>Lean <see cref="Symbol"/></returns>
        public Symbol GetSymbolFromWebsocket(string wsSymbol)
        {
            Symbol? leanSymbol;
            if (_wsSymbolMap.TryGetValue(wsSymbol, out leanSymbol))
            {
                return leanSymbol;
            }

            throw new ArgumentException($"TDAmeritrade:SymbolMapper:GetSymbolFromWebsocket(), symbol hasn't kept in collection: {wsSymbol}");
        }
    }
}
