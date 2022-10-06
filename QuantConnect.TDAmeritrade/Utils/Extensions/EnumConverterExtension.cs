using System.Reflection;
using System.Runtime.Serialization;
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

        public static string? GetEnumMemberValue<T>(this T value)
            where T : Enum
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }

        public static int ResolutionToFrequency(this Resolution resolution) => resolution switch
        {
            Resolution.Minute => 1,
            Resolution.Hour => 60,
            Resolution.Daily => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), $"Not expected Resolution value: {resolution}")
        };
    }
}
