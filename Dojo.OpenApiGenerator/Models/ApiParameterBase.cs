using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiParameterBase : IHasApiConstraints
    {
        protected readonly OpenApiParameter OpenApiParameter;

        public string Name { get; set; }
        public ApiModel ApiModel { get; set; }
        public bool IsRequired { get; }
        public abstract ParameterLocation ParameterLocation { get; }
        public string SourceCodeName { get; }

        protected ApiParameterBase(
            string sourceCodeName, 
            OpenApiParameter openApiParameter)
        {
            OpenApiParameter = openApiParameter;
            Name = openApiParameter.Name;
            ApiModel = ResolveApiModel();
            IsRequired = openApiParameter.Required;
            SourceCodeName = sourceCodeName ?? Name.ToSourceCodeName();
        }

        protected abstract ApiModel ResolveApiModel();
    }
}
