using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModel : ApiModelBase
    {
        public string Namespace { get; set; }
        public IEnumerable<ApiModelProperty> Properties { get; set; }
        public bool IsCustomModel { get; set; }
        public bool IsAbstract { get; set; }

        public ApiModel(string name,
            OpenApiSchema openApiSchema,
            string projectNamespace,
            string apiVersion,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            AutoApiGeneratorSettings autoApiGeneratorSettings) : base(openApiSchema, apiModels, apiFileName, projectNamespace, autoApiGeneratorSettings)
        {
            ProjectNamespace = projectNamespace;
            Name = name ?? openApiSchema.Title;
            TypeName = IsEnum ? Name : $"{Name}ApiModel";
            Version = apiVersion;
            SourceCodeVersion = apiVersion.ToSourceCodeVersion();
            Namespace = string.IsNullOrWhiteSpace(SourceCodeVersion)
                ? $"{ProjectNamespace}.Generated.Models"
                : $"{ProjectNamespace}.Generated.Models.V{SourceCodeVersion}";
            IsAbstract = openApiSchema.GetExtensionValueOrDefault(AutoApiGeneratorSettings.AbstractModelExtension, false);

            if (IsBuiltInType)
            {
                ResolveType(openApiSchema);
            }
            else
            {
                var properties = IsDerivedModel
                    ? openApiSchema.AllOf?.ElementAt(1)?.Properties ?? new Dictionary<string, OpenApiSchema>()
                    : openApiSchema.Properties;

                IsCustomModel = true;
                Properties = properties.Select(x => new ApiModelProperty(x.Key, x.Value, openApiSchema.Required, apiModels, apiFileName, ProjectNamespace, AutoApiGeneratorSettings));
            }
        }

        public ApiModel(OpenApiSchema openApiSchema, IDictionary<string, ApiModel> apiModels, string apiFileName, AutoApiGeneratorSettings autoApiGeneratorSettings) : base(openApiSchema, apiModels, apiFileName, string.Empty, autoApiGeneratorSettings)
        {
            if (IsBuiltInType)
            {
                ResolveType(openApiSchema);
            }
        }

        protected override string GetTypeFullName()
        {
            return !IsCustomModel ? base.GetTypeFullName() : $"{Namespace}.{TypeName}";
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
