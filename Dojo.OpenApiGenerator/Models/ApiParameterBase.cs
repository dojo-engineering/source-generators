using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiParameterBase : IHasApiConstraints
    {
        public string Version { get; }
        public IDictionary<string, ApiModel> ApiModels { get; }
        public string ApiFileName { get; }
        protected readonly OpenApiParameter OpenApiParameter;

        public string Name { get; set; }
        public ApiModel ApiModel { get; set; }
        public bool IsRequired { get; }
        public abstract ParameterLocation ParameterLocation { get; }
        public string SourceCodeName { get; }

        protected ApiParameterBase(
            string sourceCodeName, 
            OpenApiParameter openApiParameter,
            string apiVersion,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName)
        {
            Version = apiVersion;
            ApiModels = apiModels;
            ApiFileName = apiFileName;
            OpenApiParameter = openApiParameter;
            Name = openApiParameter.Name;
            ApiModel = ResolveApiModel();
            IsRequired = openApiParameter.Required;
            SourceCodeName = sourceCodeName ?? Name.ToSourceCodeParameterName();
        }

        protected abstract ApiModel ResolveApiModel();
    }
}
