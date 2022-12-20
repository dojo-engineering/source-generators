using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Extensions
{
    public static class OpenApiSchemaExtensions
    {
        public static bool IsReferenceType(this OpenApiSchema schema)
        {
            return schema.Reference != null;
        }

        public static IEnumerable<string> TryGetApiAuthorizationPolicies(this IOpenApiExtensible openApiDocument, string authPoliciesExtensionName)
        {
            if (string.IsNullOrWhiteSpace(authPoliciesExtensionName))
            {
                return null;
            }

            if (!openApiDocument.Extensions.TryGetValue(authPoliciesExtensionName, out var extension))
            {
                return null;
            }

            if (extension is not OpenApiArray extensionValues || !extensionValues.Any())
            {
                return null;
            }

            var authPolicies = new HashSet<string>();

            foreach (var extensionValue in extensionValues.Where(x => x != null))
            {
                switch (extensionValue)
                {
                    case OpenApiString openApiString:
                    {
                        if (string.IsNullOrWhiteSpace(openApiString.Value))
                        {
                            continue;
                        }

                        authPolicies.Add(openApiString.Value);

                        break;
                    }
                }
            }

            return authPolicies.Any() ? authPolicies : null;
        }

        public static HashSet<string> TryGetSupportedApiVersions(this IOpenApiExtensible openApiConfiguration, string supportedVersionExtensionName, string dateTimeVersionFormat, string declaredVersion = null)
        {
            var supportedVersions = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(declaredVersion))
            {
                supportedVersions.Add(declaredVersion);
            }

            if (string.IsNullOrWhiteSpace(supportedVersionExtensionName))
            {
                return supportedVersions;
            }

            if (!openApiConfiguration.Extensions.TryGetValue(supportedVersionExtensionName, out var extension))
            {
                return supportedVersions;
            }

            if (extension is not OpenApiArray extensionValues || !extensionValues.Any())
            {
                return supportedVersions;
            }

            foreach (var extensionValue in extensionValues.Where(x => x != null))
            {
                switch (extensionValue)
                {
                    case OpenApiString openApiString:
                    {
                        if (string.IsNullOrWhiteSpace(openApiString.Value))
                        {
                            continue;
                        }

                        supportedVersions.Add(openApiString.Value);

                        break;
                    }
                    case OpenApiDateTime openApiDateTime:
                    {
                        supportedVersions.Add(openApiDateTime.Value.ToString(dateTimeVersionFormat));

                        break;
                    }
                }
            }

            return supportedVersions.Any() ? supportedVersions : null;
        }
    }
}
