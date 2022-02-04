using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModel : ApiModelBase
    {
        private readonly string _projectNamespace;
        private readonly IDictionary<string, ApiModel> _apiModels;
        private readonly string _apiFileName;
        private string _baseModelReference;
        public string Namespace { get; set; }
        public IEnumerable<ApiModelProperty> Properties { get; set; }
        public bool IsEnum { get; }
        public IList<string> EnumValues { get; }
        public bool IsDerivedModel { get; }

        public ApiModel BaseModel => _apiModels[_baseModelReference];


        public ApiModel(
            string name, 
            OpenApiSchema openApiSchema, 
            string projectNamespace, 
            string apiVersion, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName)
        {
            _projectNamespace = projectNamespace;
            _apiModels = apiModels;
            _apiFileName = apiFileName;
            Name = name ?? openApiSchema.Title;
            IsEnum = openApiSchema.Enum.Any();
            TypeName = IsEnum ? Name : $"{Name}ApiModel";
            IsDerivedModel = TryResolveDerivedModel(openApiSchema);
            IsBuiltInType = (openApiSchema.Type != OpenApiSchemaTypes.Object || openApiSchema.AdditionalPropertiesAllowed) && !IsEnum && !IsDerivedModel;
            Version = apiVersion;
            SourceCodeVersion =  apiVersion.ToSourceCodeName();
            Namespace = string.IsNullOrWhiteSpace(SourceCodeVersion) 
                ? $"{projectNamespace}.Generated.Models"
                : $"{projectNamespace}.Generated.Models.V{SourceCodeVersion}";
            EnumValues = IsEnum ? GetEnumValues(openApiSchema.Enum).ToList() : null;

            if (IsBuiltInType)
            {
                ResolveType(openApiSchema);
            }
            else
            {
                Properties = openApiSchema.Properties.Select(x => new ApiModelProperty(x.Key, x.Value, openApiSchema.Required, apiModels, apiFileName));
            }
        }

        private bool TryResolveDerivedModel(OpenApiSchema openApiSchema)
        {
            var baseSchemaReference = openApiSchema?.AllOf?.FirstOrDefault()?.Reference;

            if (baseSchemaReference == null)
            {
                return false;
            }

            _baseModelReference = baseSchemaReference.GetApiModelReference(_apiFileName);

            return true;
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

        private IEnumerable<string> GetEnumValues(IList<IOpenApiAny> enumValues)
        {
            foreach (OpenApiString enumValue in enumValues)
            {
                yield return enumValue.Value;
            }
        }
    }
}
