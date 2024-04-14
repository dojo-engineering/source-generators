using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiConstants : BaseGeneratedCodeModel
    {
        public readonly IReadOnlyCollection<string> ApiVersions;

        public readonly IReadOnlyList<KeyValuePair<string, string>> SourceCodeApiVersionNames;

        public ApiConstants(IList<string> apiVersions, string projectNamespace) : base(projectNamespace)
        {
            ApiVersions = new ReadOnlyCollection<string>(apiVersions ?? new List<string>());
            SourceCodeApiVersionNames = new ReadOnlyCollection<KeyValuePair<string, string>>(ApiVersions.Select(ToSourceCodeApiVersionName).ToList());
        }

        private static KeyValuePair<string, string> ToSourceCodeApiVersionName(string apiVersion)
        {
            var sourceCodeName = "V" + apiVersion
                .Replace(".", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty);

            return new KeyValuePair<string, string>(sourceCodeName, apiVersion);
        }
    }
}
