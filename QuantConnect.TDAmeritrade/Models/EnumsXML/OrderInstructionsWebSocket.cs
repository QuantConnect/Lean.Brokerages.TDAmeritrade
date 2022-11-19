using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum OrderInstructionsWebSocket
    {
        [XmlEnum("Buy")]
        Buy = 0,
        [XmlEnum("Sell")]
        Sell = 1,
    }
}
