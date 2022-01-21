using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerRoute : IHasRouteParameters
    {
        public string Route { get; set; }
        public IEnumerable<ApiControllerAction> Actions { get; set; }
        public IList<ApiRouteParameter> RouteParameters { get; set; }
        public bool HasRouteParameters => RouteParameters != null && RouteParameters.Any();

        public ApiControllerRoute(
            string route,
            OpenApiPathItem openApiPathItem,
            IDictionary<string, ApiModel> apiModels,
            string projectNamespace,
            IDictionary<string, ApiParameterBase> apiParameters)
        {
            var routeParameters = openApiPathItem.Parameters.Select(p => p.GetApiParameter<ApiRouteParameter>(apiParameters, projectNamespace)).ToList();

            Route = BuildRoute(route, routeParameters);
            Actions = openApiPathItem.Operations.Select(x =>
                new ApiControllerAction(x.Key, x.Value, apiModels, routeParameters, projectNamespace, apiParameters));
            RouteParameters = routeParameters;
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
