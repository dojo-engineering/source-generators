using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiResponseContent
    {
        public string Type { get; set; }
        public ApiModel ApiModel { get; set; }

        public static ApiResponseContent Create(string contentType, OpenApiMediaType openApiMediaType, IDictionary<string, ApiModel> apiModels)
        {
            return new ApiResponseContent
            {
                Type = contentType,
                ApiModel = GetModelFullTypeName(openApiMediaType, apiModels)
            };
        }

        private static ApiModel GetModelFullTypeName(
            OpenApiMediaType openApiMediaType,
            IDictionary<string, ApiModel> apiModels)
        {
            if (openApiMediaType.Schema.Reference == null || !openApiMediaType.Schema.Reference.Type.HasValue)
            {
                return ApiModel.Create(openApiMediaType.Schema.Type, openApiMediaType.Schema.Format);
            }

            var reference = openApiMediaType.Schema.Reference.ReferenceV3;
            var schemaName = reference.Split('/').Last();

            return apiModels[schemaName];
        }
    }
}