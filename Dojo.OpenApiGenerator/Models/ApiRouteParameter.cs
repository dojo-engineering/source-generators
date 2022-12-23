using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Mvc;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiRouteParameter : ApiParameterBase
    {
        public AutoApiGeneratorSettings AutoApiGeneratorSettings { get; }
        private const string ConstraintSeparator = ":";

        public IEnumerable<string> Constraints { get; }
        public string RouteConstraintsString { get; }
        public override ParameterLocation ParameterLocation => ParameterLocation.Path;

        public ApiRouteParameter(
            string sourceCodeName, 
            OpenApiParameter openApiParameter, 
            string apiVersion,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            AutoApiGeneratorSettings autoApiGeneratorSettings) : base(sourceCodeName, openApiParameter, apiVersion, apiModels, apiFileName, autoApiGeneratorSettings)
        {
            AutoApiGeneratorSettings = autoApiGeneratorSettings;
            Constraints = GetRouteConstraints();
            RouteConstraintsString = GetRouteConstrainsString();
        }

        protected override ApiModel ResolveApiModel()
        {
            return new ApiModel(OpenApiParameter.Schema, ApiModels, ApiFileName, AutoApiGeneratorSettings);
        }

        private IEnumerable<string> GetRouteConstraints()
        {
            var routeConstraints = new List<string>();

            if (IsRequired)
            {
                routeConstraints.Add(RouteConstraints.Required);
            }

            if (ApiModel.Type == typeof(DateTime))
            {
                routeConstraints.Add(RouteConstraints.DateTime);
            }
            else if (ApiModel.Type == typeof(long))
            {
                routeConstraints.Add(RouteConstraints.Long);
            }
            else if (ApiModel.Type == typeof(int))
            {
                routeConstraints.Add(RouteConstraints.Int);
            }

            return routeConstraints.Any() ? routeConstraints : null;
        }

        private string GetRouteConstrainsString()
        {
            if (!Constraints.Any())
            {
                return Name;
            }
            var routeConstraintBuilder = new StringBuilder($"{Name}");

            foreach (var constraint in Constraints)
            {
                routeConstraintBuilder.Append($"{ConstraintSeparator}{constraint}");
            }

            return routeConstraintBuilder.ToString();
        }
    }
}
