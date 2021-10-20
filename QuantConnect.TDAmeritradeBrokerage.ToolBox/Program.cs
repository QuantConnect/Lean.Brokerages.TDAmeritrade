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

using QuantConnect.Brokerages.TDAmeritrade;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Logging;
using QuantConnect.Util;
using System;
using System.Collections.Generic;

namespace QuantConnect.TDAmeritradeDownloader.ToolBox
{
    static class Program
    {
        static void Main(string[] args)
        {
            var optionsObject = ToolboxArgumentParser.ParseArguments(args);

            if (optionsObject.Count == 0)
            {
                ApplicationParser.PrintMessageAndExit();
            }

            var fromDate = Parse.DateTimeExact(ApplicationParser.GetParameterOrExit(optionsObject, "from-date"), "yyyyMMdd-HH:mm:ss");
            var resolution = optionsObject.ContainsKey("resolution") ? optionsObject["resolution"].ToString() : "";
            var market = optionsObject.ContainsKey("market") ? optionsObject["market"].ToString() : "";
            var securityType = optionsObject.ContainsKey("security-type") ? optionsObject["security-type"].ToString() : "";
            var tickers = ToolboxArgumentParser.GetTickers(optionsObject);
            var toDate = optionsObject.ContainsKey("to-date")
                ? Parse.DateTimeExact(optionsObject["to-date"].ToString(), "yyyyMMdd-HH:mm:ss")
                : DateTime.UtcNow;

            TDAmeritradeDownloaderProgram.TDAmeritradeDownloader(tickers, resolution, fromDate, toDate, securityType);
        }
    }
}