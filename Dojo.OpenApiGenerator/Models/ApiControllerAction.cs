using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.Mvc;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerAction : IHasRouteParameters
    {
        private const string InputParametersSeparator = ",";

        public string ActionName { get; set; }
        public string HttpMethod { get; set; }
        public IEnumerable<ApiResponse> ResponseTypes { get; set; }
        public ApiResponse SuccessResponse => GetSuccessResponse();
        public IEnumerable<ApiResponse> UnsuccessfulResponses => GetUnsuccessfulResponses();
        public IEnumerable<ApiRouteParameter> RouteParameters { get; set; }
        public string InputActionParametersString => GetInputActionParametersString();
        public string InputServiceCallParametersString => GetInputServiceCallParametersString();
        public string InputServiceParametersString => GetInputServiceParametersString();

        public bool HasRouteParameters => RouteParameters != null && RouteParameters.Any();

        public static ApiControllerAction Create(
            OperationType operationType, 
            OpenApiOperation operation,
            IDictionary<string, ApiModel> apiModels, 
            IEnumerable<ApiRouteParameter> apiRouteParameters)
        {
            return new ApiControllerAction
            {
                HttpMethod = GetHttpMethodAttributeName(operationType),
                ActionName = operation.Summary,
                ResponseTypes = operation.Responses.Select(x => ApiResponse.Create(x.Key, x.Value, apiModels)),
                RouteParameters = apiRouteParameters
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

        private ApiResponse GetSuccessResponse()
        {
            if (ResponseTypes == null || !ResponseTypes.Any())
            {
                return null;
            }

            return ResponseTypes.FirstOrDefault(x => x.IsSuccessResponse);
        }

        private IEnumerable<ApiResponse> GetUnsuccessfulResponses()
        {
            return ResponseTypes.Where(x => !x.IsSuccessResponse);
        }

        private string GetInputActionParametersString()
        {
            if (!HasRouteParameters)
            {
                return null;
            }

            var actionParameterBuilder = new StringBuilder();
            var index = 0;

            foreach (var apiRouteParameter in RouteParameters)
            {
                if (index > 0)
                {
                    actionParameterBuilder.Append(" ,");
                }

                index++;

                actionParameterBuilder.Append(GetActionParameterConstraint(apiRouteParameter.IsRequired));
                actionParameterBuilder.Append(" ");
                actionParameterBuilder.Append(apiRouteParameter.ApiModel.TypeFullName);
                actionParameterBuilder.Append(" ");
                actionParameterBuilder.Append(apiRouteParameter.Name);
            }

            return actionParameterBuilder.ToString();
        }

        private string GetInputServiceCallParametersString()
        {
            if (!HasRouteParameters)
            {
                return null;
            }

            var actionParameterBuilder = new StringBuilder();
            var index = 0;

            foreach (var apiRouteParameter in RouteParameters)
            {
                if (index > 0)
                {
                    actionParameterBuilder.Append(" ,");
                }

                index++;

                actionParameterBuilder.Append(apiRouteParameter.Name);
            }

            return actionParameterBuilder.ToString();
        }

        private string GetInputServiceParametersString()
        {
            if (!HasRouteParameters)
            {
                return null;
            }

            var actionParameterBuilder = new StringBuilder();
            var index = 0;

            foreach (var apiRouteParameter in RouteParameters)
            {
                if (index > 0)
                {
                    actionParameterBuilder.Append(" ,");
                }

                index++;

                actionParameterBuilder.Append(apiRouteParameter.ApiModel.TypeFullName);
                actionParameterBuilder.Append(" ");
                actionParameterBuilder.Append(apiRouteParameter.Name);
            }

            return actionParameterBuilder.ToString();
        }

        private static string GetActionParameterConstraint(bool isRequired)
        {
            var constraint = $"[{ActionConstraints.FromRoute}";

            if (isRequired)
            {
                constraint += $", {ActionConstraints.BindRequired}]";
            }
            else
            {
                constraint += "]";
            }

            return constraint;
        }
    }
}
