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
using QuantConnect.Brokerages.TDAmeritrade;
using QuantConnect.Tests;
using System;

namespace QuantConnect.TDAmeritradeDownloader.Tests
{
    [TestFixture]
    public class TDAmeritradeBrokerageSymbolMapperTests
    {
        [Test]
        public void ReturnsCorrectLeanSymbol_Equity()
        {
            var symbolMapper = new TDAmeritradeSymbolMapper();

            var convertedSymbol = symbolMapper.GetLeanSymbol("SPY", SecurityType.Equity, Market.USA);

            Assert.AreEqual(Symbols.SPY, convertedSymbol);
        }

        [Test]
        public void ReturnsCorrectBrokerageSymbol_Equity()
        {
            var symbolMapper = new TDAmeritradeSymbolMapper();

            var convertedSymbol = symbolMapper.GetBrokerageSymbol(Symbols.SPY);

            Assert.AreEqual("SPY", convertedSymbol);
        }

        [Test]
        public void ReturnsCorrectLeanSymbol_EquityOptions()
        {
            var symbolMapper = new TDAmeritradeSymbolMapper();

            var convertedSymbol = symbolMapper.GetLeanSymbol($"SPY_{DateTime.Today:MMddyy}C1000", SecurityType.Option, Market.USA, DateTime.Today, 1000, OptionRight.Call);

            var leanSymbol = Symbol.CreateOption(Symbols.SPY, Market.USA, OptionStyle.American, OptionRight.Call, 1000, DateTime.Today);

            Assert.AreEqual(leanSymbol, convertedSymbol);
        }

        [Test]
        public void ReturnsCorrectBrokerageSymbol_EquityOptions()
        {
            var symbolMapper = new TDAmeritradeSymbolMapper();

            var leanSymbol = Symbol.CreateOption(Symbols.SPY, Market.USA, OptionStyle.American, OptionRight.Call, 1000, DateTime.Today);

            Assert.AreEqual($"SPY_{DateTime.Today:MMddyy}C1000", symbolMapper.GetBrokerageSymbol(leanSymbol));
        }
    }
}