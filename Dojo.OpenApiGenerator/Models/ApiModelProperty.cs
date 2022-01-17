using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModelProperty : ApiModelBase, IHasApiConstraints
    {
        public bool IsRequired { get; private set; }

        public static ApiModelProperty Create(string name, OpenApiSchema openApiSchema, ISet<string> required)
        {
            var model = new ApiModelProperty
            {
                Name = name,
                IsRequired = ResolveIsRequired(name, required)
            };

            model.ResolveType(openApiSchema.Type, openApiSchema.Format);

            return model;
        }

        protected override string GetTypeFullName()
        {
            return Type.FullName;
        }

        private static bool ResolveIsRequired(string name, ISet<string> requiredProperties)
        {
            return requiredProperties.Contains(name);
        }
    }
}
