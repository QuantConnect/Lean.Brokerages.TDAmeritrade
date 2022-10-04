using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.TDAmeritrade.Domain.Enums;

namespace QuantConnect.TDAmeritrade.Utils.Extensions
{
    public static class EnumConverterExtension
    {
        public static string GetProjectionTypeInRequestFormat(this ProjectionType projectType) => projectType switch
        {
            ProjectionType.SymbolSearch => "symbol-search",
            ProjectionType.Fundamental => "fundamental",
            _ => throw new ArgumentOutOfRangeException(nameof(projectType), $"Not expected direction value: {projectType}")
        };
}
}
