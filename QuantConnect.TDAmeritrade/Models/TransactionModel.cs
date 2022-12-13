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

using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public class TransactionModel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "subAccount")]
        public string SubAccount { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "settlementDate")]
        public string SettlementDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "netAmount")]
        public decimal NetAmount { get; set; }

        [JsonProperty(PropertyName = "transactionDate")]
        public string TransactionDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "orderDate")]
        public string OrderDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "transactionSubType")]
        public string TransactionSubType { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "transactionId")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "cashBalanceEffectFlag")]
        public bool CashBalanceEffectFlag { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "fees")]
        public Fees Fees { get; set; }

        [JsonProperty(PropertyName = "transactionItem")]
        public TransactionItem TransactionItem { get; set; }

        [JsonProperty(PropertyName = "instrument")]
        public InstrumentModel Instrument { get; set; } = new();
    }

    public struct Fees
    {
        [JsonProperty(PropertyName = "rFee")]
        public decimal RFee { get; set; }

        [JsonProperty(PropertyName = "additionalFee")]
        public decimal AdditionalFee { get; set; }

        [JsonProperty(PropertyName = "cdscFee")]
        public decimal CdscFee { get; set; }

        [JsonProperty(PropertyName = "regFee")]
        public decimal RegFee { get; set; }

        [JsonProperty(PropertyName = "otherCharges")]
        public decimal OtherCharges { get; set; }

        [JsonProperty(PropertyName = "commission")]
        public decimal Commission { get; set; }

        [JsonProperty(PropertyName = "optRegFee")]
        public decimal OptRegFee { get; set; }

        [JsonProperty(PropertyName = "secFee")]
        public decimal SecFee { get; set; }
    }

    public struct TransactionItem
    {
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }

        [JsonProperty(PropertyName = "cost")]
        public decimal Cost { get; set; }

        [JsonProperty(PropertyName = "instruction")]
        public InstructionType Instruction { get; set; }
    }
}
