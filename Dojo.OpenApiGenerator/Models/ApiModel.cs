using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModel : ApiModelBase
    {
        private readonly string _projectNamespace;
        public string Namespace { get; set; }
        public IEnumerable<ApiModelProperty> Properties { get; set; }
        public bool IsCustomModel { get; set; }


        public ApiModel(
            string name, 
            OpenApiSchema openApiSchema, 
            string projectNamespace, 
            string apiVersion, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName) : base(openApiSchema, apiModels, apiFileName)
        {
            _projectNamespace = projectNamespace;
            Name = name ?? openApiSchema.Title;

            TypeName = IsEnum ? Name : $"{Name}ApiModel";
            Version = apiVersion;
            SourceCodeVersion =  apiVersion.ToSourceCodeName();
            Namespace = string.IsNullOrWhiteSpace(SourceCodeVersion) 
                ? $"{projectNamespace}.Generated.Models"
                : $"{projectNamespace}.Generated.Models.V{SourceCodeVersion}";

            if (IsBuiltInType)
            {
                ResolveType(openApiSchema);
            }
            else
            {
                IsCustomModel = true;
                Properties = openApiSchema.Properties.Select(x => new ApiModelProperty(x.Key, x.Value, openApiSchema.Required, apiModels, apiFileName));
            }
        }

        public ApiModel(OpenApiSchema openApiSchema, IDictionary<string, ApiModel> apiModels, string apiFileName) : base(openApiSchema, apiModels, apiFileName)
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
