namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    /// <summary>
    /// The type of request
    /// </summary>
    public enum ProjectionType
    {
        /// <summary>
        /// Retrieve instrument data of a specific symbol or cusip
        /// </summary>
        SymbolSearch = 0,
        /// <summary>
        /// Returns fundamental data for a single instrument specified by exact symbol
        /// </summary>
        Fundamental = 1,
    }
}
