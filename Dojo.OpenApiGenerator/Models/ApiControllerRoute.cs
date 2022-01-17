using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerRoute : IHasRouteParameters
    {
        public string Route { get; set; }
        public IEnumerable<ApiControllerAction> Operations { get; set; }
        public IEnumerable<ApiRouteParameter> RouteParameters { get; set; }
        public bool HasRouteParameters => RouteParameters != null && RouteParameters.Any();

        public static ApiControllerRoute Create(string route, OpenApiPathItem openApiPathItem, IDictionary<string, ApiModel> apiModels)
        {
            var routeParameters = openApiPathItem.Parameters.Select(p => new ApiRouteParameter(p)).ToList();

            return new ApiControllerRoute
            {
                Route = BuildRoute(route, routeParameters),
                Operations = openApiPathItem.Operations.Select(x => ApiControllerAction.Create(x.Key, x.Value, apiModels, routeParameters)),
                RouteParameters = routeParameters
            };
        }

        private static string BuildRoute(string route, IList<ApiRouteParameter> routeParameters)
        {
            if (routeParameters == null || !routeParameters.Any())
            {
                return route;
            }

            foreach (var apiRouteParameter in routeParameters)
            {
                route = route.Replace($"{{{apiRouteParameter.Name}}}", $"{{{apiRouteParameter.RouteConstraintsString}}}");
            }

            return route;
        }
    }
}
