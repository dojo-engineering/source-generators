using System.Collections.Generic;
using System.Linq;
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

        public static string ToSourceCodeVersion(HashSet<string> supportedApiVersions, string version)
        {
            version = string.IsNullOrWhiteSpace(version) ? supportedApiVersions.FirstOrDefault() : version;

            return version.ToSourceCodeVersion();
        }

        public static string ToSourceCodeVersion(this string version)
        {
            version = string.IsNullOrEmpty(version) ? string.Empty :
                version
                    .Replace("-", string.Empty)
                    .Replace(".", string.Empty);

            return version.Trim('0');
        }


        public static string ToSourceCodeName(this string value, bool isPascalCase = false)
        {
            var words = value.Split('-', '.', ' ');
            var sb = new StringBuilder();

            foreach (var t in words)
            {
                sb.Append(t.Trim().FirstCharToUpper());
            }

            var sourceCodeName = sb.ToString();

            return isPascalCase ? sourceCodeName.FirstCharToUpper() : sourceCodeName.FirstCharToLower();
        }

        public static string GetApiModelKey(this string modelName, string fileName = null)
        {
            return fileName != null ? $"{fileName}_{modelName}" : modelName;
        }
    }
}
