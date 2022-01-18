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

        public string ActionName { get; }
        public string HttpMethod { get; }
        public IEnumerable<ApiResponse> ResponseTypes { get; }
        public ApiResponse SuccessResponse { get; }
        public IEnumerable<ApiResponse> UnsuccessfulResponses { get; }
        public IEnumerable<ApiRouteParameter> RouteParameters { get; }
        public string InputActionParametersString { get; }
        public string InputServiceCallParametersString { get; }
        public string InputServiceParametersString { get; }
        public bool HasUnsuccessfulResponses { get; }
        public bool HasRouteParameters { get; }

    public ApiControllerAction(
            OperationType operationType, 
            OpenApiOperation operation,
            IDictionary<string, ApiModel> apiModels, 
            IEnumerable<ApiRouteParameter> apiRouteParameters)
        {
            HttpMethod = GetHttpMethodAttributeName(operationType);
            ActionName = operation.Summary;
            ResponseTypes = operation.Responses.Select(x => ApiResponse.Create(x.Key, x.Value, apiModels));
            RouteParameters = apiRouteParameters;
            HasRouteParameters = RouteParameters != null && RouteParameters.Any();
            SuccessResponse = GetSuccessResponse();
            UnsuccessfulResponses = GetUnsuccessfulResponses();
            HasUnsuccessfulResponses = UnsuccessfulResponses != null && UnsuccessfulResponses.Any();
            InputActionParametersString = GetInputActionParametersString();
            InputServiceCallParametersString = GetInputServiceCallParametersString();
            InputServiceParametersString = GetInputServiceParametersString();
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
                    actionParameterBuilder.Append($" {InputParametersSeparator}");
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
                    actionParameterBuilder.Append($" {InputParametersSeparator}");
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
                    actionParameterBuilder.Append($" {InputParametersSeparator}");
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
                constraint += $"{InputParametersSeparator} {ActionConstraints.BindRequired}]";
            }
            else
            {
                constraint += "]";
            }

            return constraint;
        }
    }
}
