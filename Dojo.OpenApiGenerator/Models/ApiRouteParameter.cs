using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.Mvc;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiRouteParameter : ApiParameterBase
    {
        private const string ConstraintSeparator = ":";

        public IEnumerable<string> Constraints { get; }
        public string RouteConstraintsString { get; }

        public ApiRouteParameter(OpenApiParameter openApiParameter) : base(openApiParameter)
        {
            Constraints = GetRouteConstraints();
            RouteConstraintsString = GetRouteConstrainsString();
        }

        protected override ApiModel ResolveApiModel()
        {
            return ApiModel.Create(OpenApiParameter.Schema.Type, OpenApiParameter.Schema.Format);
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
