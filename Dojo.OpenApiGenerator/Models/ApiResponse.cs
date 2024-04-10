using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiResponse
    {
        public string Version { get; }
        public string ApiFileName { get; }
        public int HttpStatusCode { get; set; }
        public IEnumerable<ApiContent> ContentTypes { get; set; }
        public ApiModel ApiModel => GetResponseApiModel();
        public bool IsSuccessResponse => HttpStatusCode is >= 200 and <= 299;
        public bool IsBadRequestResponse => HttpStatusCode == 400;
        public bool IsNotFoundResponse => HttpStatusCode == 404;
        public bool HasNoContent => HttpStatusCode == 204;

        public ApiResponse(
            string statusCode, 
            OpenApiResponse openApiResponse, 
            IDictionary<string, ApiModel> apiModels, 
            string version,
            string apiFileName)
        {
            Version = version;
            ApiFileName = apiFileName;
            HttpStatusCode = int.Parse(statusCode);

            if (openApiResponse.Content != null && openApiResponse.Content.Any())
            {
                ContentTypes = openApiResponse
                    .Content
                    .Select(x => new ApiContent(x.Key, x.Value, apiModels, version, apiFileName));
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