using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerDefinition
    {
        public string ProjectNamespace { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public IEnumerable<ApiControllerRoute> Routes { get; set; }
        public IDictionary<string, ApiModel> Models { get; set; }
        public bool CanOverride { get; set; }
        public bool OverrideResponses { get; set; }
    }
}
