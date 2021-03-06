using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerDefinition
    {
        public string ProjectNamespace { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string SourceCodeVersion { get; set; }
        public IEnumerable<ApiControllerRoute> Routes { get; set; }
        public bool CanOverride { get; set; }
        public IDictionary<string, ApiParameterBase> Parameters { get; set; }
    }
}
