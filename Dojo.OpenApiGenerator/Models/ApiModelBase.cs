using System;
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

        protected static Type GetBuiltInType(string openApiType, string openApiTypeFormat)
        {
            switch (openApiType)
            {
                case OpenApiSchemaTypes.Integer:
                {
                    return typeof(int);
                }
                case OpenApiSchemaTypes.String:
                {
                    if (string.IsNullOrWhiteSpace(openApiTypeFormat))
                    {
                        return typeof(string);
                    }

                    switch (openApiTypeFormat)
                    {
                        case OpenApiTypeFormats.Date:
                        case OpenApiTypeFormats.DateTime:
                        {
                            return typeof(DateTime);
                        }
                    }

                    break;
                }
            }

            return null;
        }

        protected abstract string GetTypeFullName();
    }
}
