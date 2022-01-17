using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiParameterBase : IHasApiConstraints
    {
        protected readonly OpenApiParameter OpenApiParameter;

        public string Name { get; set; }
        public ApiModel ApiModel { get; set; }

        public bool IsRequired { get; }

        protected ApiParameterBase(OpenApiParameter openApiParameter)
        {
            OpenApiParameter = openApiParameter;
            Name = openApiParameter.Name;
            ApiModel = ResolveApiModel();
            IsRequired = openApiParameter.Required;
        }

        protected abstract ApiModel ResolveApiModel();
    }
}
