using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiResponse
    {
        public string Version { get; }
        public string ApiFileName { get; }
        public AutoApiGeneratorSettings AutoApiGeneratorSettings { get; }
        public int HttpStatusCode { get; set; }
        public IEnumerable<ApiContent> ContentTypes { get; set; }
        public ApiModel ApiModel => GetResponseApiModel();
        public bool IsSuccessResponse => HttpStatusCode is >= 200 and <= 299;
        public bool IsBadRequestResponse => HttpStatusCode == StatusCodes.Status400BadRequest;
        public bool IsNotFoundResponse => HttpStatusCode == StatusCodes.Status404NotFound;
        public bool HasNoContent => HttpStatusCode == StatusCodes.Status204NoContent;

        public ApiResponse(
            string statusCode, 
            OpenApiResponse openApiResponse, 
            IDictionary<string, ApiModel> apiModels, 
            string version,
            string apiFileName,
            AutoApiGeneratorSettings autoApiGeneratorSettings)
        {
            Version = version;
            ApiFileName = apiFileName;
            AutoApiGeneratorSettings = autoApiGeneratorSettings;
            HttpStatusCode = int.Parse(statusCode);

            if (openApiResponse.Content != null && openApiResponse.Content.Any())
            {
                ContentTypes = openApiResponse
                    .Content
                    .Select(x => new ApiContent(x.Key, x.Value, apiModels, version, apiFileName, AutoApiGeneratorSettings));
            }
        }

        private ApiModel GetResponseApiModel()
        {
            if (ContentTypes == null || !ContentTypes.Any())
            {
                return null;
            }

            return ContentTypes.Select(x => x.ApiModel).Where(x => x != null).Distinct().First();
        }
    }
}