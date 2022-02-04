using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Models;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Extensions
{
    internal static class OpenApiParameterExtensions
    {
        public static T GetApiParameter<T>(this OpenApiParameter openApiParameter,
            string apiVersion,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            IDictionary<string, ApiParameterBase> globalParameters = null,
            string projectNamespace = null) where T : ApiParameterBase
        {
            if (openApiParameter.Reference != null)
            {
                var parameterName = openApiParameter.Reference.GetModelName();
                return globalParameters[parameterName] as T;
            }

            return GetApiParameterInternal(null, openApiParameter, apiVersion, projectNamespace, apiModels, apiFileName) as T;
        }

        public static IDictionary<string, ApiParameterBase> GetApiParameters(
            this IDictionary<string, OpenApiParameter> openApiParameters,
            string projectNamespace, 
            Dictionary<string, ApiModel> apiModels,
            string apiVersion,
            string apiFileName)
        {
            var apiParameters = new Dictionary<string, ApiParameterBase>();
            foreach (var (sourceCodeName, openApiParameter) in openApiParameters.Select(x => (x.Key, x.Value)))
            {
                apiParameters.Add(sourceCodeName, GetApiParameterInternal(sourceCodeName, openApiParameter, apiVersion, projectNamespace, apiModels, apiFileName));
            }

            return apiParameters;
        }

        private static ApiParameterBase GetApiParameterInternal(string sourceCodeName,
            OpenApiParameter apiParameter,
            string apiVersion,
            string projectNamespace, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName)
        {
            switch (apiParameter.In)
            {
                case ParameterLocation.Path:
                {
                    return new ApiRouteParameter(sourceCodeName, apiParameter, apiVersion, apiModels, apiFileName);
                }

                case ParameterLocation.Header:
                {
                    return new ApiHeaderParameter(sourceCodeName, apiParameter, projectNamespace, apiModels, apiVersion, apiFileName);
                }
                case ParameterLocation.Query:
                {
                    return new ApiQueryParameter(sourceCodeName, apiParameter, projectNamespace, apiModels, apiVersion, apiFileName);
                }
            }

            return null;
        }
    }
}
