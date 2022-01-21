using System;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiQueryParameter : ApiParameterBase
    {
        public override ParameterLocation ParameterLocation => ParameterLocation.Query;

        public ApiQueryParameter(string sourceCodeName, OpenApiParameter openApiParameter) : base(sourceCodeName, openApiParameter)
        {
        }

        protected override ApiModel ResolveApiModel()
        {
            throw new NotImplementedException();
        }
    }
}
