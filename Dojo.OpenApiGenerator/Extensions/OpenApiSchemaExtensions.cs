﻿using System;
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

        public static HashSet<string> TryGetSupportedApiVersions(this IOpenApiExtensible openApiConfiguration, string supportedVersionExtensionName, string dateTimeVersionFormat, string declaredVersion = null, IList<string> defaultApiVersions = null)
        {
            var supportedVersions = new HashSet<string>();

            if (defaultApiVersions != null && defaultApiVersions.Any())
            {
                supportedVersions.UnionWith(defaultApiVersions);
            }

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

        public static T GetExtensionValueOrDefault<T>(this IOpenApiExtensible schema, string extensionName, T defaultValue)
        {
            if (!schema.Extensions.TryGetValue(extensionName, out var extension))
            {
                return defaultValue;
            }

            object value = null;

            if (typeof(T) == typeof(string))
            {
                value = (extension as OpenApiString)?.Value;
            }
            else if (typeof(T) == typeof(int))
            {
                value = (extension as OpenApiInteger)?.Value;
            }
            else if (typeof(T) == typeof(bool))
            {
                value = (extension as OpenApiBoolean)?.Value;
            }
            else if (typeof(T) == typeof(DateTime))
            {
                value = (extension as OpenApiDateTime)?.Value;
            }

            if (value == null)
            {
                return defaultValue;
            }

            return (T)value;
        }
    }
}
