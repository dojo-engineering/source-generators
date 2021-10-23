using System;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    internal static class ConventionalTypeMapper
    {
        internal static string GetTypeFromName(string name)
        {
            if (name.IndexOf("url", StringComparison.OrdinalIgnoreCase) > -1 ||
               name.IndexOf("uri", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return nameof(Uri);
            }

            if (name.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return nameof(TimeSpan);
            }

            if (name.IndexOf("date", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return nameof(DateTime);
            }

            // Not conventional
            return null;
        }
    }
}
