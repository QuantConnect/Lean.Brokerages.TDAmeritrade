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

        public static string GetEnumValue(this Enum value)
        {
            // Get the Description attribute value for the enum value
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (EnumMemberAttribute[])fi.GetCustomAttributes(typeof(EnumMemberAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Value;
            }
            else
            {
                return value.ToString();
            }
        }

        public static T ToEnum<T>(this string str)
        {
            var enumType = typeof(T);
            foreach (var name in Enum.GetNames(enumType))
            {
                var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
                if (enumMemberAttribute.Value == str) return (T)Enum.Parse(enumType, name);
            }
            //throw exception or whatever handling you want or
            return default(T);
        }
    }
}
