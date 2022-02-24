using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModelProperty : ApiModelBase, IHasApiConstraints
    {
        public bool IsRequired { get; }
        public string SourceName { get; }

        public ApiModelProperty(
            string name, 
            OpenApiSchema openApiSchema, 
            ISet<string> required, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName) : base(openApiSchema, apiModels, apiFileName)
        {
            Name = name;
            SourceName = name.FirstCharToUpper();
            IsRequired = ResolveIsRequired(name, required);

            ResolveType(openApiSchema);
        }

        private static bool ResolveIsRequired(string name, ISet<string> requiredProperties)
        {
            return requiredProperties != null && requiredProperties.Contains(name);
        }
    }
}
