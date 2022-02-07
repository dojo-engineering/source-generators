using System.Text;

namespace Dojo.OpenApiGenerator.Extensions
{
    internal static class StringHelpers
    {
        public static string FirstCharToUpper(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return char.ToUpper(value[0]) + value.Substring(1);
        }

        public static string FirstCharToLower(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return char.ToLower(value[0]) + value.Substring(1);
        }

        public static string ToSourceCodeName(this string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : 
                value
                    .Replace("-", string.Empty)
                    .Replace(".", string.Empty);
        }

        public static string ToSourceCodeParameterName(this string value)
        {
            var words = value.Split('-', '.');
            var sb = new StringBuilder(words[0]);

            for (var i = 1; i < words.Length; i++)
            {
                sb.Append(words[i].FirstCharToUpper());
            }

            return sb.ToString().FirstCharToLower();
        }

        public static string GetApiModelKey(this string modelName, string fileName = null)
        {
            return fileName != null ? $"{fileName}_{modelName}" : modelName;
        }
    }
}
