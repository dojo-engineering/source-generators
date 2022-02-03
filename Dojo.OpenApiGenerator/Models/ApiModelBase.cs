using System;
using System.Collections.Generic;
using Dojo.OpenApiGenerator.OpenApi;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiModelBase
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public Type Type { get; set; }
        public string TypeFullName => GetTypeFullName();
        public bool IsBuiltInType { get; set; }
        public bool IsEmail { get; private set; }
        public IList<Type> InnerTypes { get; private set; }
        public string Version { get; set; }
        public string SourceCodeVersion { get; set; }

        protected void ResolveType(OpenApiSchema openApiSchema)
        {
            if (openApiSchema.Type == null)
            {
                Type = typeof(object);

                return;
            }

            var openApiType = openApiSchema.Type;
            var openApiTypeFormat = openApiSchema.Format;

            switch (openApiType)
            {
                case OpenApiSchemaTypes.Object:
                {
                    if (!openApiSchema.AdditionalPropertiesAllowed)
                    {
                        Type = typeof(object);
                    }
                    else
                    {
                        ResolveDictionaryType(openApiSchema.AdditionalProperties);
                    }

                    break;
                }
                case OpenApiSchemaTypes.Integer:
                    {
                        if (string.IsNullOrWhiteSpace(openApiTypeFormat))
                        {
                            Type = typeof(int);
                        }

                        switch (openApiTypeFormat)
                        {
                            case OpenApiTypeFormats.Int32:
                                {
                                    Type = typeof(int);

                                    break;
                                }
                            case OpenApiTypeFormats.Int64:
                                {
                                    Type = typeof(long);

                                    break;
                                }
                            default:
                                {
                                    Type = typeof(int);

                                    break;
                                }
                        }

                        break;
                    }
                case OpenApiSchemaTypes.String:
                    {
                        if (string.IsNullOrWhiteSpace(openApiTypeFormat))
                        {
                            Type = typeof(string);
                        }

                        switch (openApiTypeFormat)
                        {
                            case OpenApiTypeFormats.Date:
                            case OpenApiTypeFormats.DateTime:
                                {
                                    Type = typeof(DateTime);

                                    break;
                                }
                            case OpenApiTypeFormats.Email:
                            {
                                IsEmail = true;
                                Type = typeof(string);

                                break;
                            }
                            default:
                                {
                                    Type = typeof(string);

                                    break;
                                }
                        }

                        break;
                    }
            }
        }

        private void ResolveDictionaryType(OpenApiSchema additionalProperties)
        {
            //TODO resolve reference types
            var dictArgType1 = typeof(string);
            var dictArgType2 = new ApiModel(additionalProperties).Type;

            InnerTypes = new List<Type>
            {
                dictArgType1, 
                dictArgType2
            };
            Type = typeof(IDictionary<,>);
        }

        protected abstract string GetTypeFullName();
    }
}
