using System.Collections.Generic;
using Dojo.OpenApiGenerator.Configuration;
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
            string projectNamespace,
            AutoApiGeneratorSettings autoApiGeneratorSettings) : base(openApiSchema, apiModels, apiFileName, projectNamespace, autoApiGeneratorSettings)
        {
            Name = name;
            SourceName = name.FirstCharToUpper();
            IsRequired = ResolveIsRequired(name, required);
            ProjectNamespace = projectNamespace;

            ResolveType(openApiSchema);
        }

        protected override string GetTypeFullName()
        {
            var typeFullName = base.GetTypeFullName();

            if (IsReferenceType && !IsRequired && ReferenceModel.IsEnum)
            {
                return  $"System.Nullable<{typeFullName}>";
            }

            return typeFullName;
        }

        private static bool ResolveIsRequired(string name, ISet<string> requiredProperties)
        {
            return requiredProperties != null && requiredProperties.Contains(name);
        }
    }
}
