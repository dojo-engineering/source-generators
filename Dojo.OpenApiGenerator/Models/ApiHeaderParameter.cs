using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiHeaderParameter : ApiParameterBase
    {
        private readonly string _projectNamespace;
        private readonly Dictionary<string, ApiModel> _apiModels;

        public override ParameterLocation ParameterLocation => ParameterLocation.Header;

        public ApiHeaderParameter(
            string sourceCodeName, 
            OpenApiParameter openApiParameter, 
            string projectNamespace, 
            Dictionary<string, ApiModel> apiModels) : base(sourceCodeName, openApiParameter)
        {
            _projectNamespace = projectNamespace;
            _apiModels = apiModels;
        }

        protected override ApiModel ResolveApiModel()
        {
            return OpenApiParameter.Schema.Reference != null ? 
                _apiModels[OpenApiParameter.Reference.GetModelName()] : 
                new ApiModel(null, OpenApiParameter.Schema, _projectNamespace);
        }
    }
}
