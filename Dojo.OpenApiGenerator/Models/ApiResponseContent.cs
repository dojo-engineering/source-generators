using System.Collections.Generic;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiResponseContent
    {
        public string Type { get; set; }
        public ApiModel ApiModel { get; set; }

        public ApiResponseContent(string contentType, OpenApiMediaType openApiMediaType, IDictionary<string, ApiModel> apiModels)
        {
            Type = contentType;
            ApiModel = GetModelFullTypeName(openApiMediaType, apiModels);
        }

        private static ApiModel GetModelFullTypeName(
            OpenApiMediaType openApiMediaType,
            IDictionary<string, ApiModel> apiModels)
        {
            if (!openApiMediaType.Schema.IsReferenceType())
            {
                return new ApiModel(openApiMediaType.Schema);
            }

            var refName = openApiMediaType.Schema.Reference.GetModelName();

            return apiModels[refName];
        }
    }
}