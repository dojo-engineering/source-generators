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

        public ApiModel(string name, OpenApiSchema openApiSchema, string projectNamespace)
        {
            Name = name ?? openApiSchema.Title;
            TypeName = $"{openApiSchema.Title}ApiModel";
            IsBuiltInType = openApiSchema.Type != OpenApiSchemaTypes.Object || openApiSchema.AdditionalPropertiesAllowed;
            Namespace = $"{projectNamespace}.Generated.Models";

            if (IsBuiltInType)
            {
                ResolveType(openApiSchema);
            }
            else
            {
                Properties = openApiSchema.Properties.Select(x => new ApiModelProperty(x.Key, x.Value, openApiSchema.Required));
            }

            //SetRequiredProperties(model, openApiSchema.Required);
        }

        public ApiModel(OpenApiSchema openApiSchema)
        {
            IsBuiltInType = true;
            ResolveType(openApiSchema);
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
