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
        public InstrumentTransaction Instrument { get; set; } = new();
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

    public struct InstrumentTransaction
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "cusip")]
        public string Cusip { get; set; }

        [JsonProperty(PropertyName = "assetType")]
        public string AssetType { get; set; }
    }
}
