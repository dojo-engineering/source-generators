using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModel : ApiModelBase
    {
        public string Namespace { get; set; }
        public IEnumerable<ApiModelProperty> Properties { get; set; }
        public bool IsCustomModel { get; set; }

        public bool IsDictionary { get; set; }

        public ApiModel DictionaryValueType { get; set; }

        public ApiModel(
            string name, 
            OpenApiSchema openApiSchema, 
            string projectNamespace, 
            string apiVersion, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName) : base(openApiSchema, apiModels, apiFileName, projectNamespace)
        {
            ProjectNamespace = projectNamespace;
            Name = name ?? openApiSchema.Title;
            TypeName = IsEnum ? Name : $"{Name}ApiModel";
            Version = apiVersion;
            SourceCodeVersion =  apiVersion.ToSourceCodeVersion();
            Namespace = string.IsNullOrWhiteSpace(SourceCodeVersion) 
                ? $"{ProjectNamespace}.Generated.Models"
                : $"{ProjectNamespace}.Generated.Models.V{SourceCodeVersion}";

            if (IsBuiltInType)
            {
                ResolveType(openApiSchema);
            }
            else
            {
                IsCustomModel = true;
                IsDictionary = IsDictionaryType(openApiSchema);

                if (!IsDictionary)
                {
                    Properties = openApiSchema.Properties.Select(x => new ApiModelProperty(x.Key, x.Value, openApiSchema.Required, apiModels, apiFileName, ProjectNamespace));
                }
                else
                {
                    DictionaryValueType = new ApiModel(openApiSchema.AdditionalProperties, apiModels, apiFileName);
                }
            }
        }

        public ApiModel(OpenApiSchema openApiSchema, IDictionary<string, ApiModel> apiModels, string apiFileName, string name = null) : base(openApiSchema, apiModels, apiFileName, string.Empty)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }

            if (IsBuiltInType)
            {

                ResolveType(openApiSchema);
            }
        }

        protected override string GetTypeFullName()
        {
            return !IsCustomModel ? base.GetTypeFullName() : $"{Namespace}.{TypeName}";
        }

        private static bool IsDictionaryType(OpenApiSchema openApiSchema)
        {
            return openApiSchema.AdditionalPropertiesAllowed &&
                   openApiSchema.AdditionalProperties != null &&
                   (openApiSchema.Properties == null || openApiSchema.Properties.Count == 0);
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
