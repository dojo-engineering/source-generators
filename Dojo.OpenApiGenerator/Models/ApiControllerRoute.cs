using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerRoute
    {
        public string Route { get; set; }
        public IEnumerable<ApiControllerOperation> Operations { get; set; }

        public static ApiControllerRoute Create(string route, OpenApiPathItem openApiPathItem, IDictionary<string, ApiModel> apiModels)
        {
            return new ApiControllerRoute
            {
                Route = route,
                Operations = openApiPathItem.Operations.Select(x => ApiControllerOperation.Create(x.Key, x.Value, apiModels))
            };
        }
    }
}
