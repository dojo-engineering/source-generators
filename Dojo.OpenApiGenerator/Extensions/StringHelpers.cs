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

        public static string ToSourceCodeName(this string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Trim('-', ' ');
        }
    }
}
