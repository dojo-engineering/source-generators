using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Mvc;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerAction : IHasRouteParameters, IHasHeaderParameters, IHasRequestBody, IHasQueryParameters
    {
        private readonly IDictionary<string, ApiModel> _apiModels;
        private readonly string _projectNamespace;
        private readonly IDictionary<string, ApiParameterBase> _apiParameters;
        private readonly string _apiFileName;
        private readonly AutoApiGeneratorSettings _apiGeneratorSettings;
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
        public bool HasRouteParameters { get; private set; }
        public IList<ApiHeaderParameter> HeaderParameters { get; private set; }
        public bool HasHeaderParameters { get; private set; }
        public ApiRequestBody RequestBody { get; }
        public bool HasRequestBody { get; }
        public List<ApiParameterBase> AllParameters { get; private set; }
        public string ContentTypesStringList => GetContentTypesAsList();
        public IList<ApiQueryParameter> QueryParameters { get; private set; }
        public bool HasQueryParameters { get; private set; }
        public bool HasAnyParameters { get; private set; }
        public bool IsDeprecated { get; }

        public ApiControllerAction(
            OperationType operationType,
            OpenApiOperation operation,
            IDictionary<string, ApiModel> apiModels,
            IList<ApiRouteParameter> apiRouteParameters,
            string projectNamespace,
            IDictionary<string, ApiParameterBase> apiParameters,
            string apiVersion,
            string apiFileName,
            AutoApiGeneratorSettings apiGeneratorSettings)
        {
            _apiModels = apiModels;
            _projectNamespace = projectNamespace;
            _apiParameters = apiParameters;
            _apiFileName = apiFileName;
            _apiGeneratorSettings = apiGeneratorSettings;
            IsDeprecated = operation.Deprecated;
            HttpMethod = GetHttpMethodAttributeName(operationType);
            ActionName = operation.Summary;
            ResponseTypes = operation.Responses.Select(x => new ApiResponse(x.Key, x.Value, apiModels, apiVersion, apiFileName));
            RouteParameters = apiRouteParameters;
            Version = apiVersion;
            SuccessResponse = GetSuccessResponse();
            UnsuccessfulResponses = GetUnsuccessfulResponses();
            HasUnsuccessfulResponses = UnsuccessfulResponses != null && UnsuccessfulResponses.Any();
            RequestBody = GetRequestBody(operation.RequestBody, apiModels);
            HasRequestBody = RequestBody != null;

            ResolveParameters(operation.Parameters);

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

        private void ResolveParameters(IList<OpenApiParameter> operationParameters)
        {
            AllParameters = new List<ApiParameterBase>();

            if (RouteParameters != null && RouteParameters.Any())
            {
                HasAnyParameters = true;
                HasRouteParameters = true;
                AllParameters.AddRange(RouteParameters);
            }

            foreach (var operationParameter in operationParameters.OrderBy(x => x.In))
            {
                switch (operationParameter.In)
                {
                    case ParameterLocation.Query:
                        {
                            QueryParameters ??= new List<ApiQueryParameter>();

                            var parameter = operationParameter.GetApiParameter<ApiQueryParameter>(Version, _apiModels, _apiFileName, _apiParameters, _projectNamespace);

                            QueryParameters.Add(parameter);
                            AllParameters.Add(parameter);
                            HasQueryParameters = true;
                            HasAnyParameters = true;

                            break;
                        }
                    case ParameterLocation.Header:
                        {
                            HeaderParameters ??= new List<ApiHeaderParameter>();

                            var parameter = operationParameter.GetApiParameter<ApiHeaderParameter>(Version, _apiModels, _apiFileName, _apiParameters, _projectNamespace);

                            HeaderParameters.Add(parameter);
                            AllParameters.Add(parameter);
                            HasHeaderParameters = true;
                            HasAnyParameters = true;

                            break;
                        }
                }
            }
        }

        private static string GetHttpMethodAttributeName(OperationType operationType)
        {
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
                    if (ExcludeVersionParameter(apiParameter))
                    {
                        continue;
                    }

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

        private bool ExcludeVersionParameter(ApiParameterBase apiParameter)
        {
            return !_apiGeneratorSettings.IncludeVersionParameterInActionSignature &&
                   apiParameter.Name == _apiGeneratorSettings.VersionParameterName;
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
                    if (ExcludeVersionParameter(apiParameter))
                    {
                        continue;
                    }

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
                    if (ExcludeVersionParameter(apiParameter))
                    {
                        continue;
                    }

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
                case ParameterLocation.Query:
                    return ActionConstraints.FromQuery;
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
