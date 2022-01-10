using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiResponse
    {
        public int HttpStatusCode { get; set; }
        public IEnumerable<ApiResponseContent> ContentTypes { get; set; }
        public IEnumerable<ApiModel> ResponseModelTypes => GetResponseModelTypes();

        public string ContentTypesStringList => GetContentTypesAsList();

        public static ApiResponse Create(string statusCode, OpenApiResponse openApiResponse, IDictionary<string, ApiModel> apiModels)
        {
            var response = new ApiResponse
            {
                HttpStatusCode = int.Parse(statusCode)
            };

            if (openApiResponse.Content != null && openApiResponse.Content.Any())
            {
                response.ContentTypes =
                    openApiResponse.Content.Select(x => ApiResponseContent.Create(x.Key, x.Value, apiModels));
            }
            
            return response;
        }

        private string GetContentTypesAsList()
        {
            if (ContentTypes == null || !ContentTypes.Any())
            {
                return null;
            }

            return string.Join(",", ContentTypes.Select(x => $"\"{x.Type}\""));
        }

        private IEnumerable<ApiModel> GetResponseModelTypes()
        {
            if (ContentTypes == null || !ContentTypes.Any())
            {
                return null;
            }

            return ContentTypes.Select(x => x.ApiModel).Distinct();
        }
    }
}