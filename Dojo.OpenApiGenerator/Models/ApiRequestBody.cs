using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiRequestBody : IHasApiConstraints
    {
        private readonly IDictionary<string, ApiModel> _apiModels;
        private readonly string _apiFileName;
        private readonly OpenApiRequestBody _openApiRequestBody;

        public string Version { get; }
        public string Name { get; set; }
        public ApiModel ApiModel { get; set; }
        public bool IsRequired { get; }
        public string SourceCodeName { get; }

        public ApiRequestBody(
            OpenApiRequestBody openApiRequestBody,
            IDictionary<string, ApiModel> apiModels,
            string apiVersion,
            string apiFileName)
        {
            _apiModels = apiModels;
            _apiFileName = apiFileName;
            _openApiRequestBody = openApiRequestBody;

            Version = apiVersion;
            IsRequired = _openApiRequestBody.Required;
            ApiModel = ResolveApiModel();
            Name = openApiRequestBody.Description ?? ApiModel.Name;
            SourceCodeName = Name.ToSourceCodeParameterName();
        }

        private ApiModel ResolveApiModel()
        {
            //TODO add support multiple request body types
            var content = _openApiRequestBody.Content.FirstOrDefault();
            var apiContent = new ApiContent(content.Key, content.Value, _apiModels, Version, _apiFileName);

            return apiContent.ApiModel;
        }
    }
}
