using System;
using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiModelBase
    {
        protected string BaseModelReference;
        protected string ModelReference { get; set; }
        protected OpenApiSchema OpenApiSchema { get; }
        protected IDictionary<string, ApiModel> ApiModels { get; }
        protected string ApiFileName { get; }

        public bool IsEnum { get; }
        public IList<string> EnumValues { get; }
        public bool IsDerivedModel { get; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public Type Type { get; set; }
        public string TypeFullName => GetTypeFullName();
        public bool IsBuiltInType { get; set; }
        public bool IsEmail { get; private set; }
        public IList<ApiModel> InnerTypes { get; private set; }
        public string Version { get; set; }
        public string SourceCodeVersion { get; set; }
        public ApiModel BaseModel => ApiModels[BaseModelReference];
        public bool IsReferenceType { get; }
        public ApiModel ReferenceModel => ApiModels[ModelReference];

        protected ApiModelBase(
            OpenApiSchema openApiSchema, 
            IDictionary<string, ApiModel> apiModels,
            string apiFileName)
        {
            OpenApiSchema = openApiSchema.OneOf != null && openApiSchema.OneOf.Any() ? openApiSchema.OneOf.FirstOrDefault() : openApiSchema;
            ApiModels = apiModels;
            ApiFileName = apiFileName;
            IsEnum = OpenApiSchema.Enum.Any();
            IsDerivedModel = TryResolveDerivedModel(OpenApiSchema);
            IsReferenceType = TryResolveReferenceModel(OpenApiSchema);
            IsBuiltInType = (OpenApiSchema.Type != OpenApiSchemaTypes.Object || OpenApiSchema.AdditionalPropertiesAllowed) && !IsEnum && !IsDerivedModel && !IsReferenceType;
            EnumValues = IsEnum ? GetEnumValues(OpenApiSchema.Enum).ToList() : null;
            
        }

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
                    ResolveObject(openApiSchema);

                    break;
                }
                case OpenApiSchemaTypes.Integer:
                {
                    ResolveInteger(openApiTypeFormat);

                    break;
                }
                case OpenApiSchemaTypes.String:
                {
                    ResolveString(openApiTypeFormat);

                    break;
                }
                case OpenApiSchemaTypes.Array:
                {
                    ResolveArray(openApiSchema);

                    break;
                }
            }
        }

        private IEnumerable<string> GetEnumValues(IList<IOpenApiAny> enumValues)
        {
            foreach (OpenApiString enumValue in enumValues)
            {
                yield return enumValue.Value;
            }
        }

        private void ResolveArray(OpenApiSchema openApiSchema)
        {
            var arrayItemType = new ApiModel(openApiSchema.Items, ApiModels, ApiFileName);
            
            InnerTypes = new List<ApiModel>
            {
                arrayItemType
            };

            Type = typeof(IEnumerable<>);
        }

        private void ResolveString(string openApiTypeFormat)
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
        }

        private void ResolveInteger(string openApiTypeFormat)
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
        }

        private void ResolveObject(OpenApiSchema openApiSchema)
        {
            if (!openApiSchema.AdditionalPropertiesAllowed)
            {
                Type = typeof(object);
            }
            else
            {
                ResolveDictionaryType(openApiSchema.AdditionalProperties);
            }
        }

        private void ResolveDictionaryType(OpenApiSchema additionalProperties)
        {
            //TODO resolve reference types
            var dictArgType1 = new ApiModel(new OpenApiSchema { Type = OpenApiSchemaTypes.String}, ApiModels, ApiFileName);
            var dictArgType2 = new ApiModel(additionalProperties, ApiModels, ApiFileName);

            InnerTypes = new List<ApiModel>
            {
                dictArgType1, 
                dictArgType2
            };
            Type = typeof(IDictionary<,>);
        }

        private bool TryResolveDerivedModel(OpenApiSchema openApiSchema)
        {
            var baseSchemaReference = openApiSchema?.AllOf?.FirstOrDefault()?.Reference;

            if (baseSchemaReference == null)
            {
                return false;
            }

            BaseModelReference = baseSchemaReference.GetApiModelReference(ApiFileName);

            return true;
        }

        private bool TryResolveReferenceModel(OpenApiSchema openApiSchema)
        {
            var isReference = openApiSchema.IsReferenceType();

            if (!isReference)
            {
                return false;
            }

            ModelReference = openApiSchema.Reference.GetApiModelReference(ApiFileName);

            return true;
        }

        protected virtual string GetTypeFullName()
        {
            if (IsReferenceType)
            {
                return ReferenceModel.GetTypeFullName();
            }

            if (Type == typeof(IDictionary<,>))
            {
                return $"{Type.Namespace}.IDictionary<{InnerTypes[0].TypeFullName},{InnerTypes[1].TypeFullName}>";
            }

            if (Type == typeof(IEnumerable<>))
            {
                return $"{Type.Namespace}.IEnumerable<{InnerTypes[0].TypeFullName}>";
            }

            return Type?.FullName;
        }
    }
}
