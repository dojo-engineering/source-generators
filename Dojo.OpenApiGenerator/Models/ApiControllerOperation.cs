using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerOperation
    {
        public string OperationName { get; set; }
        public string HttpMethod { get; set; }
        public IEnumerable<ApiResponse> ResponseTypes { get; set; }

        public static ApiControllerOperation Create(OperationType operationType, OpenApiOperation operation, IDictionary<string, ApiModel> apiModels)
        {
            return new ApiControllerOperation
            {
                HttpMethod = GetHttpMethodAttributeName(operationType),
                OperationName = operation.Summary,
                ResponseTypes = operation.Responses.Select(x => ApiResponse.Create(x.Key, x.Value, apiModels))
            };
        }

        private static string GetHttpMethodAttributeName(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Get:  return "HttpGet";
            }

            return null;
        }
    }
}
