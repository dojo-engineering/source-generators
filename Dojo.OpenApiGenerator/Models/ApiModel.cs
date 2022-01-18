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

        public static ApiModel Create(OpenApiSchema openApiSchema, string projectNamespace)
        {
            var model = new ApiModel
            {
                Name = openApiSchema.Title,
                TypeName = $"{openApiSchema.Title}ApiModel",
                IsBuiltInType = openApiSchema.Type != OpenApiSchemaTypes.Object,
                Namespace = $"{projectNamespace}.Generated.Models"
            };

            if (model.IsBuiltInType)
            {
                model.ResolveType(openApiSchema.Type, openApiSchema.Format);
            }
            else
            {
                model.Properties = openApiSchema.Properties.Select(x => ApiModelProperty.Create(x.Key, x.Value, openApiSchema.Required));
            }

            //SetRequiredProperties(model, openApiSchema.Required);

            return model;
        }

        public static ApiModel Create(string openApiSchemaType, string openApiSchemaFormat)
        {
            var model = new ApiModel
            {
                IsBuiltInType = true
            };

            model.ResolveType(openApiSchemaType, openApiSchemaFormat);

            return model;
        }

        protected override string GetTypeFullName()
        {
            return IsBuiltInType ? Type.FullName : $"{Namespace}.{TypeName}";
        }

        //private static void SetRequiredProperties(ApiModel apiModel, ISet<string> requiredProperties)
        //{
        //    if (requiredProperties == null || !requiredProperties.Any())
        //    {
        //        foreach (var requiredProperty in requiredProperties)
        //        {
        //            apiModel.Properties.First(x => x.Name == requiredProperty).IsRequired = true;
        //        }
        //    }
        //}
    }
}
