using System;
using System.Collections.Generic;
using Dojo.OpenApiGenerator.OpenApi;

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

        protected void ResolveType(string openApiType, string openApiTypeFormat)
        {
            switch (openApiType)
            {
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
                                    ;
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

        protected abstract string GetTypeFullName();
    }
}
