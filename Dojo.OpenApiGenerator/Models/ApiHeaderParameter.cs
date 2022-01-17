using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiHeaderParameter : ApiParameterBase
    {
        public ApiHeaderParameter(OpenApiParameter openApiParameter) : base(openApiParameter)
        {
        }

        protected override ApiModel ResolveApiModel()
        {
            throw new System.NotImplementedException();
        }
    }
}
