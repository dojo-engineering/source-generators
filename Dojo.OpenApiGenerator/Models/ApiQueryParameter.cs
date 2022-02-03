using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiQueryParameter : ApiParameterBase
    {
        public override ParameterLocation ParameterLocation => ParameterLocation.Query;

        public ApiQueryParameter(
            string sourceCodeName, 
            OpenApiParameter openApiParameter, 
            string apiVersion,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName) : base(sourceCodeName, openApiParameter, apiVersion, apiModels, apiFileName)
        {
        }

        protected override ApiModel ResolveApiModel()
        {
            throw new NotImplementedException();
        }
    }
}
