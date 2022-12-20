using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerDefinition : BaseGeneratedCodeModel, IHasAuthorizationPolicies, IHasVersions
    {
        public string Title { get; set; }
        public HashSet<string> SupportedVersions { get; set; }
        public string SourceCodeVersion { get; set; }
        public IEnumerable<ApiControllerRoute> Routes { get; set; }
        public bool CanOverride { get; set; }
        public IDictionary<string, ApiParameterBase> Parameters { get; set; }
        public IEnumerable<string> AuthorizationPolicies { get; set; }

        public ApiControllerDefinition(string projectNamespace) : base(projectNamespace)
        {
        }
    }
}
