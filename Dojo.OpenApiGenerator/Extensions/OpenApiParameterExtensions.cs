using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Models;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Extensions
{
    internal static class OpenApiParameterExtensions
    {
        public static T GetApiParameter<T>(this OpenApiParameter openApiParameter,
            IDictionary<string, ApiParameterBase> globalParameters = null,
            string projectNamespace = null) where T : ApiParameterBase
        {
            if (openApiParameter.Reference != null)
            {
                var parameterName = openApiParameter.Reference.GetModelName();
                return globalParameters[parameterName] as T;
            }

            return GetApiParameter(null, openApiParameter, projectNamespace) as T;
        }

        public static IDictionary<string, ApiParameterBase> GetApiParameters(
            this IDictionary<string, OpenApiParameter> openApiParameters,
            string projectNamespace, 
            Dictionary<string, ApiModel> apiModels)
        {
            var apiParameters = new Dictionary<string, ApiParameterBase>();
            foreach (var (sourceCodeName, openApiParameter) in openApiParameters.Select(x => (x.Key, x.Value)))
            {
                apiParameters.Add(sourceCodeName, GetApiParameter(sourceCodeName, openApiParameter, projectNamespace, apiModels));
            }

            return apiParameters;
        }

        private static ApiParameterBase GetApiParameter(string sourceCodeName,
            OpenApiParameter apiParameter,
            string projectNamespace = null, 
            Dictionary<string, ApiModel> apiModels = null)
        {
            switch (apiParameter.In)
            {
                case ParameterLocation.Path:
                {
                    return new ApiRouteParameter(sourceCodeName, apiParameter);
                }

                case ParameterLocation.Header:
                {
                    return new ApiHeaderParameter(sourceCodeName, apiParameter, projectNamespace, apiModels);
                }
            }

            return null;
        }
    }
}
