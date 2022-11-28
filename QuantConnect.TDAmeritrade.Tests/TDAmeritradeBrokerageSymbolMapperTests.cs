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

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    [TestFixture]
    public class TDAmeritradeBrokerageSymbolMapperTests
    {
        [TestCase("AAPL", "AAPL")]
        [TestCase("VXXB", "VXXB")]
        public void MapCorrectBrokerageSymbol(string ticker, string wexSymbol)
        {
            var mapper = new TDAmeritradeSymbolMapper(TestGlobals.MapFileProvider);

            var symbol = Symbol.Create(ticker, SecurityType.Equity, Market.USA);
            var brokerageSymbol = mapper.GetBrokerageSymbol(symbol);
            Assert.That(brokerageSymbol, Is.EqualTo(wexSymbol));
        }

        [TestCase("NVAX", SecurityType.Equity, "NVAX")]
        public void ReturnsCorrectBrokerageSymbol(string symbolValue, SecurityType symbolSecurityType, string expectedBrokerageSymbol)
        {
            var leanSymbol = Symbol.Create(symbolValue, symbolSecurityType, Market.USA);

            var symbolMapper = new TDAmeritradeSymbolMapper(TestGlobals.MapFileProvider);

            var symbolBrokerage = symbolMapper.GetBrokerageSymbol(leanSymbol);

            Assert.That(symbolBrokerage, Is.EqualTo(expectedBrokerageSymbol));
        }
    }
}
