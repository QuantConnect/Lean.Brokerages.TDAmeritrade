using Newtonsoft.Json;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public struct ExchangeAgreementsModel
    {
        [JsonProperty(PropertyName = "NASDAQ_EXCHANGE_AGREEMENT")]
        public string NasdaqExchangeAgreement { get; set; }

        [JsonProperty(PropertyName = "NYSE_EXCHANGE_AGREEMENT")]
        public string NyseExchangeAgreement { get; set; }

        [JsonProperty(PropertyName = "OPRA_EXCHANGE_AGREEMENT")]
        public string OpraExchangeAgreement { get; set; }
    }
}
