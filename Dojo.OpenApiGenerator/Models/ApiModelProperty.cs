using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModelProperty : ApiModelBase, IHasApiConstraints
    {
        private readonly OpenApiSchema _openApiSchema;
        private readonly IDictionary<string, ApiModel> _apiModels;
        private readonly string _apiFileName;
        public bool IsRequired { get; }
        public string SourceName { get; }

        public ApiModelProperty(
            string name, 
            OpenApiSchema openApiSchema, 
            ISet<string> required, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName)
        {
            _openApiSchema = openApiSchema;
            _apiModels = apiModels;
            _apiFileName = apiFileName;

            Name = name;
            SourceName = name.FirstCharToUpper();
            IsRequired = ResolveIsRequired(name, required);

            ResolveType(openApiSchema);
        }

        protected override string GetTypeFullName()
        {
            if (_openApiSchema.IsReferenceType())
            {

                var refName = _openApiSchema.Reference.GetApiModelReference(_apiFileName);

                return _apiModels[refName].TypeFullName;
            }

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
