using System;
using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerRoute : IHasRouteParameters
    {
        public string Version { get; }
        public string Route { get; set; }
        public IEnumerable<ApiControllerAction> Actions { get; set; }
        public IList<ApiRouteParameter> RouteParameters { get; set; }
        public bool HasRouteParameters => RouteParameters != null && RouteParameters.Any();

        public ApiControllerRoute(
            string route,
            OpenApiPathItem openApiPathItem,
            IDictionary<OperationType, OpenApiOperation> operations,
            IDictionary<string, ApiModel> apiModels,
            string projectNamespace,
            IDictionary<string, ApiParameterBase> apiParameters,
            string apiVersion,
            string apiFileName,
            AutoApiGeneratorSettings apiGeneratorSettings)
        {
            Version = apiVersion;
            var routeParameters = openApiPathItem.Parameters.Select(p => p.GetApiParameter<ApiRouteParameter>(Version, apiModels, apiFileName, apiParameters, projectNamespace)).ToList();

            Route = BuildRoute(route, routeParameters);
            Actions = operations.Select(x =>
                new ApiControllerAction(Route, x.Key, x.Value, apiModels, routeParameters, projectNamespace, apiParameters, apiVersion, apiFileName, apiGeneratorSettings));
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
                if (apiRouteParameter == null)
                {
                    Console.WriteLine($"Could not generate parameter for route '${route}', value is null. Check OpenApi specification configuration.");
                    continue;
                }

                route = route.Replace($"{{{apiRouteParameter.Name}}}", $"{{{apiRouteParameter.RouteConstraintsString}}}");
            }

            return route;
        }
    }
}
