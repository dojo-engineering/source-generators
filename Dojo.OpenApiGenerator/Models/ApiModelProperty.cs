using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModelProperty : ApiModelBase, IHasApiConstraints
    {
        public bool IsRequired { get; }
        public string SourceName { get; }

        public ApiModelProperty(string name, OpenApiSchema openApiSchema, ISet<string> required)
        {
            Name = name;
            SourceName = name.FirstCharToUpper();
            IsRequired = ResolveIsRequired(name, required);

            ResolveType(openApiSchema);
        }

        protected override string GetTypeFullName()
        {
            return Type == typeof(IDictionary<,>) ? 
                $"{Type.Namespace}.IDictionary<{InnerTypes[0].FullName},{InnerTypes[1].FullName}>" : 
                Type.FullName;
        }

        private static bool ResolveIsRequired(string name, ISet<string> requiredProperties)
        {
            return requiredProperties != null && requiredProperties.Contains(name);
        }
    }
}
