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
using QuantConnect.Configuration;
using QuantConnect.Brokerages.TDAmeritrade;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    public class TDAmeritradeBrokerageRestAPITests
    {
        private TDAmeritradeBrokerage _brokerage;
        private string _apiKey;

        [SetUp]
        public void Setup()
        {
            var required = new[] { "tdameritrade-api-key", "tdameritrade-account-number" };

            foreach (var item in required)
            {
                if (string.IsNullOrEmpty(Config.Get(item)))
                {
                    throw new ArgumentException($"TDAmeritradeBrokerageFactory.CreateBrokerage: Missing {item} in config.json");
                }
            }

            _apiKey = Config.Get("tdameritrade-api-key");
            string _codeFromUrl = Config.Get("tdameritrade-access-token");
            string _accountNumber = Config.Get("tdameritrade-account-number");

            _brokerage = new TDAmeritradeBrokerage(_apiKey, _codeFromUrl, _accountNumber, null, null);
        }

        [Explicit("This test requires a configured and testable account")]
        [Test]
        public void GetSignInUrl()
        {
            var url = _brokerage.GetSignInUrl();

            QuantConnect.Logging.Log.Trace($"TDAmeritradeBrokerage: URL: {url}");

            Assert.That(url, Is.Not.Null);
            Assert.That(url, Is.Not.Empty);
            Assert.That(url, Does.Contain(_apiKey));
        }

        [Explicit("This test requires a configured and testable account")]
        [TestCase(true, "LODE", "AAPL", "IBM")]
        [TestCase(false, "LODEE", "AAPLA", "IBMA")]
        public void GetAsk(bool expected, params string[] symbols)
        {
            var quotes = _brokerage.GetQuotes(symbols);
            Assert.That(quotes.Count() == symbols.Length, Is.EqualTo(expected));
        }
    }
}
