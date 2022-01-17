using System;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiQueryParameter : ApiParameterBase
    {
        public ApiQueryParameter(OpenApiParameter openApiParameter) : base(openApiParameter)
        {
        }

        protected override ApiModel ResolveApiModel()
        {
            throw new NotImplementedException();
        }
    }
}
