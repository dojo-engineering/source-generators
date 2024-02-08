using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Mvc;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiControllerAction : IHasRouteParameters, IHasHeaderParameters, IHasRequestBody, IHasQueryParameters, IHasAuthorizationPolicies
    {
        private readonly OpenApiOperation _operation;
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
        public IList<ApiRouteParameter> RouteParameters { get; private set; }
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

        public IEnumerable<string> AuthorizationPolicies { get; set; }

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
            _operation = operation;
            _apiModels = apiModels;
            _projectNamespace = projectNamespace;
            _apiParameters = apiParameters;
            _apiFileName = apiFileName;
            _apiGeneratorSettings = apiGeneratorSettings;
            IsDeprecated = operation.Deprecated;
            HttpMethod = GetHttpMethodAttributeName(operationType);
            ActionName = ToActionName(!string.IsNullOrWhiteSpace(operation.OperationId) ? operation.OperationId : operation.Summary);
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
            AuthorizationPolicies = _operation.TryGetApiAuthorizationPolicies(apiGeneratorSettings.ApiAuthorizationPoliciesExtension);
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

        private void ResolveParameters(IEnumerable<OpenApiParameter> operationParameters)
        {
            AllParameters = new List<ApiParameterBase>();

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
                    case ParameterLocation.Path:
                        {
                            RouteParameters ??= new List<ApiRouteParameter>();

                            if (RouteParameters.Any(p => p.Name == operationParameter.Name))
                            {
                                break;
                            }

                            var parameter = operationParameter.GetApiParameter<ApiRouteParameter>(Version, _apiModels, _apiFileName, _apiParameters, _projectNamespace);

                            RouteParameters.Add(parameter);

                            break;
                        }
                }
            }

            if (RouteParameters != null && RouteParameters.Any())
            {
                HasAnyParameters = true;
                HasRouteParameters = true;
                AllParameters.AddRange(RouteParameters);
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
            if (!HasAnyParameters && !HasRequestBody)
            {
                return null;
            }

            var actionParameterBuilder = new StringBuilder();

            if (HasRequestBody)
            {
                actionParameterBuilder.Append(
                    GetParameterSignature(
                        GetActionBodyParameterConstraint(RequestBody),
                    RequestBody.ApiModel.TypeFullName,
                    RequestBody.SourceCodeName));
            }

            var parametersWithoutVersion = AllParameters
                .Where(p => !ExcludeVersionParameter(p))
                .OrderBy(p => p.ApiModel.DefaultValue != null)
                .ToList();

            if (!parametersWithoutVersion.Any())
            {
                return actionParameterBuilder.ToString();
            }

            if (actionParameterBuilder.Length > 0)
            {
                actionParameterBuilder.Append($"{InputParametersSeparator}");
            }

            var index = 0;

            foreach (var apiParameter in parametersWithoutVersion)
            {
                if (index > 0 && index < parametersWithoutVersion.Count)
                {
                    actionParameterBuilder.Append($"{InputParametersSeparator}");
                }

                index++;

                var parameterConstraints = GetActionParameterConstraints(apiParameter);
                var parameterSignature = GetParameterSignature(parameterConstraints, apiParameter.ApiModel.TypeFullName, apiParameter.SourceCodeName);

                actionParameterBuilder.Append(parameterSignature);

                TryAppendParameterDefaultValue(apiParameter, actionParameterBuilder);
            }
            AppendCancellationTokenParameterSignature(actionParameterBuilder);

            return actionParameterBuilder.ToString();
        }

        private bool ExcludeVersionParameter(ApiParameterBase apiParameter)
        {
            return !_apiGeneratorSettings.IncludeVersionParameterInActionSignature &&
                   apiParameter.Name == _apiGeneratorSettings.VersionParameterName;
        }

        private string GetInputServiceCallParametersString()
        {
            if (!HasAnyParameters && !HasRequestBody)
            {
                return null;
            }

            var builder = new StringBuilder();
            var index = 0;

            var parametersWithoutVersion = AllParameters
                .Where(p => !ExcludeVersionParameter(p))
                .ToList();

            foreach (var apiParameter in parametersWithoutVersion)
            {
                if (index > 0 && index < parametersWithoutVersion.Count)
                {
                    builder.Append($"{InputParametersSeparator}");
                }

                index++;

                builder.Append(apiParameter.SourceCodeName);
            }

            if (HasRequestBody)
            {
                if (builder.Length > 0)
                {
                    builder.Append($"{InputParametersSeparator}");
                }

                builder.Append(RequestBody.SourceCodeName);
            }

            AppendCancellationTokenParameter(builder);

            return builder.ToString();
        }

        private string GetInputServiceParametersString()
        {
            if (!HasAnyParameters && !HasRequestBody)
            {
                return null;
            }

            var builder = new StringBuilder();
            var index = 0;
            var parametersWithoutVersion = AllParameters
                .Where(p => !ExcludeVersionParameter(p))
                .ToList();

            foreach (var apiParameter in parametersWithoutVersion)
            {
                if (index > 0 && index < parametersWithoutVersion.Count)
                {
                    builder.Append($"{InputParametersSeparator}");
                }

                index++;

                builder.Append(apiParameter.ApiModel.TypeFullName);
                builder.Append(" ");
                builder.Append(apiParameter.SourceCodeName);
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

            AppendCancellationTokenParameterSignature(builder);

            return builder.ToString();
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

        private static string GetActionParameterConstraints(ApiParameterBase apiParameter)
        {
            var actionSourceConstraint = GetActionParameterSourceConstraint(apiParameter.ParameterLocation, apiParameter.Name);
            var constraintBuilder = new StringBuilder($"[{actionSourceConstraint}");
            var constraints = new List<string>();

            if (apiParameter.IsRequired)
            {
                constraints.Add(ActionConstraints.BindRequired);
            }

            if (apiParameter.MinLength.HasValue)
            {
                constraints.Add(ActionConstraints.MinLength);
            }

            if (apiParameter.MaxLength.HasValue)
            {
                constraints.Add(ActionConstraints.MaxLength);
            }

            if (constraints.Any())
            {
                foreach (var constraint in constraints)
                {
                    constraintBuilder.Append(InputParametersSeparator);

                    switch (constraint)
                    {
                        case ActionConstraints.BindRequired:
                            {
                                constraintBuilder.Append($"{ActionConstraints.BindRequired}");
                                break;
                            }
                        case ActionConstraints.MinLength:
                            {
                                constraintBuilder.Append($"{ActionConstraints.MinLength}({apiParameter.MinLength})");
                                break;
                            }
                        case ActionConstraints.MaxLength:
                            {
                                constraintBuilder.Append($"{ActionConstraints.MaxLength}({apiParameter.MaxLength})");
                                break;
                            }
                    }
                }
            }

            constraintBuilder.Append("]");

            return constraintBuilder.ToString();
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

        private static string GetActionParameterSourceConstraint(ParameterLocation parameterLocation, string parameterName)
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

        private static string ToActionName(string actionName)
        {
            var actionNameWords = actionName.Split('_', '-', ' ');

            return string.Join("", actionNameWords.Select(w => w.FirstCharToUpper()));
        }

        private static void TryAppendParameterDefaultValue(ApiParameterBase apiParameter, StringBuilder actionParameterBuilder)
        {
            if (apiParameter.ApiModel.DefaultValue == null)
            {
                return;
            }

            actionParameterBuilder.Append(" = ");
            var defaultValue = apiParameter.ApiModel.DefaultValue.ToString();

            switch (apiParameter.ApiModel.DefaultValue)
            {
                case bool:
                    {
                        actionParameterBuilder.Append($"{defaultValue.ToLower()}");
                        break;
                    }
                case string:
                    {
                        actionParameterBuilder.Append($"\"{defaultValue}\"");
                        break;
                    }
                default:
                    {
                        actionParameterBuilder.Append(defaultValue);
                        break;
                    }
            }
        }

        private static string GetParameterSignature(string constraints, string typeFullName, string parameterName)
        {
            return $"{constraints} {typeFullName} {parameterName}";
        }

        private static void AppendCancellationTokenParameterSignature(StringBuilder builder)
            =>  builder.Append(builder.Length > 0 ? $"{InputParametersSeparator}CancellationToken cancellationToken" : "CancellationToken cancellationToken");

        private static void AppendCancellationTokenParameter(StringBuilder builder)
            =>  builder.Append(builder.Length > 0 ? $"{InputParametersSeparator}cancellationToken" : "cancellationToken");
    }
}
