using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiConstants : BaseGeneratedCodeModel
    {
        public readonly IReadOnlyCollection<string> ApiVersions;

        public ApiConstants(IList<string> apiVersions, string projectNamespace) : base(projectNamespace)
        {
            ApiVersions = new ReadOnlyCollection<string>(apiVersions);
        }
    }
}
