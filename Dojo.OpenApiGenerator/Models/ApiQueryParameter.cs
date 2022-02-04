using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiQueryParameter : ApiParameterBase
    {
        private readonly string _projectNamespace;
        public override ParameterLocation ParameterLocation => ParameterLocation.Query;

        public ApiQueryParameter(
            string sourceCodeName, 
            OpenApiParameter openApiParameter,
            string projectNamespace,
            IDictionary<string, ApiModel> apiModels,
            string apiVersion,
            string apiFileName) : base(sourceCodeName, openApiParameter, apiVersion, apiModels, apiFileName)
        {
            _projectNamespace = projectNamespace;
        }

        protected override ApiModel ResolveApiModel()
        {
            return OpenApiParameter.Schema.Reference != null ?
                ApiModels[OpenApiParameter.Reference.GetModelName()] :
                new ApiModel(null, OpenApiParameter.Schema, _projectNamespace, Version, ApiModels, ApiFileName);
        }
    }
}
