using QuantConnect.Brokerages.TDAmeritrade.Models;

namespace QuantConnect.Brokerages.TDAmeritrade.Utils
{
    public static class WebSocketConverterExtension
    {
        public static int ConvertQuantityExchangeToQC(decimal quantity, OrderInstructionsWebSocket orderInstructions)
        {
            switch (orderInstructions)
            {
                case OrderInstructionsWebSocket.Buy:
                    return (int)quantity;
                case OrderInstructionsWebSocket.Sell:
                    return -(int)quantity;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
