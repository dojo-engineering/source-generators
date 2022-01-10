using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.OpenApi;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModel : ApiModelBase
    {
        public string Namespace { get; set; }
        public IEnumerable<ApiModelProperty> Properties { get; set; }

        public static ApiModel Create(string name, OpenApiSchema openApiSchema, string projectNamespace)
        {
            var model = new ApiModel
            {
                Name = name,
                TypeName = $"{name}ApiModelGenerated",
                IsBuiltInType = openApiSchema.Type != OpenApiSchemaTypes.Object,
                Namespace = $"{projectNamespace}.Models"
            };

            if (model.IsBuiltInType)
            {
                model.Type = GetBuiltInType(openApiSchema.Type, openApiSchema.Format);
            }
            else
            {
                model.Properties = openApiSchema.Properties.Select(x => ApiModelProperty.Create(x.Key, x.Value));
            }

            return model;
        }

        public static ApiModel Create(string openApiSchemaType, string openApiSchemaFormat)
        {
            var model = new ApiModel
            {
                Type = GetBuiltInType(openApiSchemaType, openApiSchemaFormat)
            };

            return model;
        }

        protected override string GetTypeFullName()
        {
            return IsBuiltInType ? Type.FullName : $"{Namespace}.{TypeName}";
        }
    }
}
