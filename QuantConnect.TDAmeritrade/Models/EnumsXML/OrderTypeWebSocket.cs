using System.Xml.Serialization;

namespace QuantConnect.Brokerages.TDAmeritrade.Models
{
    public enum OrderTypeWebSocket
    {
        [XmlEnum("Market")]
        Market = 0,
        [XmlEnum("Limit")]
        Limit = 1,
        [XmlEnum("Stop")]
        Stop = 2,
        [XmlEnum("Stop Limit")]
        StopLimit = 3,
    }
}
