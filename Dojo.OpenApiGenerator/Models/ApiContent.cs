using System.Collections.Generic;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiContent
    {
        private readonly IDictionary<string, ApiModel> _apiModels;
        private readonly string _apiFileName;

        public string Type { get; set; }
        public string Version { get; }
        public ApiModel ApiModel { get; set; }

        public ApiContent(
            string contentType, 
            OpenApiMediaType openApiMediaType, 
            IDictionary<string, ApiModel> apiModels, 
            string apiVersion,
            string apiFileName, 
            AutoApiGeneratorSettings autoApiGeneratorSettings)
        {
            _apiModels = apiModels;
            _apiFileName = apiFileName;
            Type = contentType;
            Version = apiVersion;
            ApiModel = GetApiModel(openApiMediaType, apiModels, autoApiGeneratorSettings);
        }

        private ApiModel GetApiModel(
            OpenApiMediaType openApiMediaType,
            IDictionary<string, ApiModel> apiModels,
            AutoApiGeneratorSettings autoApiGeneratorSettings)
        {
            if (!openApiMediaType.Schema.IsReferenceType())
            {
                return new ApiModel(openApiMediaType.Schema, _apiModels, _apiFileName, autoApiGeneratorSettings);
            }

            var refName = openApiMediaType.Schema.Reference.GetApiModelReference(_apiFileName);

            return apiModels[refName];
        }
    }
}