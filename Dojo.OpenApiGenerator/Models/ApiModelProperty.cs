using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModelProperty : ApiModelBase, IHasApiConstraints
    {
        public bool IsRequired { get; }
        public string SourceName { get; }

        public ApiModelProperty(string name,
            OpenApiSchema openApiSchema,
            ISet<string> required,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            string projectNamespace) : base(openApiSchema, apiModels, apiFileName, projectNamespace)
        {
            Name = name;
            SourceName = name.FirstCharToUpper();
            IsRequired = ResolveIsRequired(name, required);
            ProjectNamespace = projectNamespace;

            ResolveType(openApiSchema);
        }

        private static bool ResolveIsRequired(string name, ISet<string> requiredProperties)
        {
            return requiredProperties != null && requiredProperties.Contains(name);
        }
    }
}
