using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Mvc;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerAction : IHasRouteParameters, IHasHeaderParameters, IHasRequestBody
    {
        private readonly IDictionary<string, ApiModel> _apiModels;
        private readonly string _projectNamespace;
        private readonly IDictionary<string, ApiParameterBase> _apiParameters;
        private readonly string _apiFileName;
        private const string InputParametersSeparator = ", ";

        public string ActionName { get; }
        public string HttpMethod { get; }
        public IEnumerable<ApiResponse> ResponseTypes { get; }
        public ApiResponse SuccessResponse { get; }
        public IEnumerable<ApiResponse> UnsuccessfulResponses { get; }
        public IList<ApiRouteParameter> RouteParameters { get; }
        public string Version { get; }
        public string InputActionParametersString { get; }
        public string InputServiceCallParametersString { get; }
        public string InputServiceParametersString { get; }
        public bool HasUnsuccessfulResponses { get; }
        public bool HasRouteParameters { get; }
        public IList<ApiHeaderParameter> HeaderParameters { get; }
        public bool HasHeaderParameters { get; }
        public ApiRequestBody RequestBody { get; }
        public bool HasRequestBody { get; }
        public IList<ApiParameterBase> AllParameters { get; }
        public string ContentTypesStringList => GetContentTypesAsList();
        public bool HasAnyParameters { get; }

        public ApiControllerAction(
            OperationType operationType, 
            OpenApiOperation operation,
            IDictionary<string, ApiModel> apiModels, 
            IList<ApiRouteParameter> apiRouteParameters,
            string projectNamespace,
            IDictionary<string, ApiParameterBase> apiParameters,
            string apiVersion,
            string apiFileName)
        {
            _apiModels = apiModels;
            _projectNamespace = projectNamespace;
            _apiParameters = apiParameters;
            _apiFileName = apiFileName;
            HttpMethod = GetHttpMethodAttributeName(operationType);
            ActionName = operation.Summary;
            ResponseTypes = operation.Responses.Select(x => new ApiResponse(x.Key, x.Value, apiModels, apiVersion, apiFileName));
            RouteParameters = apiRouteParameters;
            Version = apiVersion;
            HasRouteParameters = RouteParameters != null && RouteParameters.Any();
            SuccessResponse = GetSuccessResponse();
            UnsuccessfulResponses = GetUnsuccessfulResponses();
            HasUnsuccessfulResponses = UnsuccessfulResponses != null && UnsuccessfulResponses.Any();
            HeaderParameters = GetHeaderParameters(operation.Parameters).ToList();
            HasHeaderParameters = HeaderParameters != null && HeaderParameters.Any();
            RequestBody = GetRequestBody(operation.RequestBody, apiModels);
            HasRequestBody = RequestBody != null;
            AllParameters = GetAllParameters();
            HasAnyParameters = AllParameters != null && AllParameters.Any();
            InputActionParametersString = GetInputActionParametersString();
            InputServiceCallParametersString = GetInputServiceCallParametersString();
            InputServiceParametersString = GetInputServiceParametersString();
        }

        private ApiRequestBody GetRequestBody(OpenApiRequestBody operationRequestBody, IDictionary<string, ApiModel> apiModels)
        {
            var content = operationRequestBody?.Content?.FirstOrDefault();
            if (content == null)
            {
                return null;
            }

            return new ApiRequestBody(operationRequestBody, apiModels, Version, _apiFileName);
        }

        private IList<ApiParameterBase> GetAllParameters()
        {
            IList<ApiParameterBase> allParameters = null;

            if (HasRouteParameters)
            {
                allParameters = new List<ApiParameterBase>(RouteParameters);
            }

            if (HasHeaderParameters)
            {
                allParameters ??= new List<ApiParameterBase>();
                allParameters = allParameters.Concat(HeaderParameters).ToList();
            }

            return allParameters;
        }

        private IEnumerable<ApiHeaderParameter> GetHeaderParameters(IList<OpenApiParameter> operationParameters)
        {
            if (operationParameters == null || !operationParameters.Any())
            {
                yield break;
            }

            foreach (var operationParameter in operationParameters.Where(x => x.In == ParameterLocation.Header))
            {
                yield return operationParameter.GetApiParameter<ApiHeaderParameter>(Version, _apiModels, _apiFileName, _apiParameters, _projectNamespace);
            }
        }

        private static string GetHttpMethodAttributeName(OperationType operationType)
        {
            //switch (operationType)
            //{
            //    case OperationType.Get:  return "HttpGet";
            //    case OperationType.Post: return "HttpPost";
            //    case OperationType.Delete: return "HttpDelete";
            //}

            return $"Http{operationType}";
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
            if (!HasAnyParameters)
            {
                return null;
            }

            var actionParameterBuilder = new StringBuilder();
            var index = 0;

            if (HasAnyParameters)
            {
                foreach (var apiParameter in AllParameters)
                {
                    if (index > 0)
                    {
                        actionParameterBuilder.Append($"{InputParametersSeparator}");
                    }

                    index++;

                    actionParameterBuilder.Append(GetActionParameterConstraint(apiParameter));
                    actionParameterBuilder.Append(" ");
                    actionParameterBuilder.Append(apiParameter.ApiModel.TypeFullName);
                    actionParameterBuilder.Append(" ");
                    actionParameterBuilder.Append(apiParameter.SourceCodeName);
                }
            }

            if (HasRequestBody)
            {
                if (actionParameterBuilder.Length > 0)
                {
                    actionParameterBuilder.Append($"{InputParametersSeparator}");
                }

                actionParameterBuilder.Append(GetActionBodyParameterConstraint(RequestBody));
                actionParameterBuilder.Append(" ");
                actionParameterBuilder.Append(RequestBody.ApiModel.TypeFullName);
                actionParameterBuilder.Append(" ");
                actionParameterBuilder.Append(RequestBody.SourceCodeName);
            }

            return actionParameterBuilder.ToString();
        }

        private string GetInputServiceCallParametersString()
        {
            if (!HasAnyParameters)
            {
                return null;
            }

            var builder = new StringBuilder();
            var index = 0;

            if (HasAnyParameters)
            {
                foreach (var apiParameter in AllParameters)
                {
                    if (index > 0)
                    {
                        builder.Append($"{InputParametersSeparator}");
                    }

                    index++;

                    builder.Append(apiParameter.SourceCodeName);
                }
            }

            if (HasRequestBody)
            {
                if (builder.Length > 0)
                {
                    builder.Append($"{InputParametersSeparator}");
                }

                builder.Append(RequestBody.SourceCodeName);
            }

            return builder.ToString();
        }

        private string GetInputServiceParametersString()
        {
            if (!HasAnyParameters)
            {
                return null;
            }

            var builder = new StringBuilder();
            var index = 0;

            if (HasAnyParameters)
            {
                foreach (var apiParameter in AllParameters)
                {
                    if (index > 0)
                    {
                        builder.Append($"{InputParametersSeparator}");
                    }

                    index++;

                    builder.Append(apiParameter.ApiModel.TypeFullName);
                    builder.Append(" ");
                    builder.Append(apiParameter.SourceCodeName);
                }
            }

            if (HasRequestBody)
            {
                if (builder.Length > 0)
                {
                    builder.Append($"{InputParametersSeparator}");
                }

                builder.Append(RequestBody.ApiModel.TypeFullName);
                builder.Append(" ");
                builder.Append(RequestBody.SourceCodeName);
            }

            return builder.ToString();
        }

        private static string GetActionParameterConstraint(ApiParameterBase apiParameter)
        {
            var actionSourceConstraint = GetActionParameterSourceConstrain(apiParameter.ParameterLocation, apiParameter.Name);
            var constraint = $"[{actionSourceConstraint}";

            if (apiParameter.IsRequired)
            {
                constraint += $"{InputParametersSeparator}{ActionConstraints.BindRequired}]";
            }
            else
            {
                constraint += "]";
            }

            return constraint;
        }

        private static string GetActionBodyParameterConstraint(ApiRequestBody apiRequestBody)
        {
            var actionSourceConstraint = ActionConstraints.FromBody;
            var constraint = $"[{actionSourceConstraint}";

            if (apiRequestBody.IsRequired)
            {
                constraint += $"{InputParametersSeparator}{ActionConstraints.BindRequired}]";
            }
            else
            {
                constraint += "]";
            }

            return constraint;
        }

        private static string GetActionParameterSourceConstrain(ParameterLocation parameterLocation, string parameterName)
        {
            switch (parameterLocation)
            {
                case ParameterLocation.Path:
                    return ActionConstraints.FromRoute;
                case ParameterLocation.Header:
                    return GetFromHeaderActionConstraint(parameterName);
            }

            return null;
        }

        private static string GetFromHeaderActionConstraint(string parameterName)
        {
            return $"{ActionConstraints.FromHeader}(Name = \"{parameterName}\")";
        }

        private string GetContentTypesAsList()
        {
            var contentTypes = new HashSet<string>();

            if (ResponseTypes == null)
            {
                return null;
            }

            foreach (var responseType in ResponseTypes)
            {
                if (responseType?.ContentTypes == null)
                {
                    continue;
                }

                foreach (var contentType in responseType.ContentTypes)
                {
                    if (contentType?.Type != null)
                    {
                        contentTypes.Add(contentType.Type);
                    }
                }
            }

            return contentTypes.Any() ?
                string.Join(",", contentTypes.Select(x => $"\"{x}\"")) :
                null;
        }
    }
}
