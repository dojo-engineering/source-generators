﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiModelBase : BaseGeneratedCodeModel
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
        public ApiModel BaseModel => !string.IsNullOrWhiteSpace(BaseModelReference) ? ApiModels[BaseModelReference] : null;
        public bool IsReferenceType { get; }
        public ApiModel ReferenceModel => !string.IsNullOrWhiteSpace(ModelReference) ? ApiModels[ModelReference] : null;
        public object DefaultValue { get; private set; }
        public bool IsNullable { get; private set; }
        public int? MaxLength { get; }
        public int? MinLength { get; }

        protected ApiModelBase(
            OpenApiSchema openApiSchema,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            string projectNamespace) : base(projectNamespace)
        {
            OpenApiSchema = openApiSchema?.OneOf != null && openApiSchema.OneOf.Any() ? openApiSchema.OneOf.FirstOrDefault() : openApiSchema;
            ApiModels = apiModels;
            ApiFileName = apiFileName;
            IsEnum = OpenApiSchema != null && OpenApiSchema.Enum.Any();
            IsDerivedModel = TryResolveDerivedModel(OpenApiSchema);
            IsReferenceType = TryResolveReferenceModel(OpenApiSchema);
            IsBuiltInType = (OpenApiSchema?.Type != OpenApiSchemaTypes.Object || OpenApiSchema.AdditionalPropertiesAllowed) && !IsEnum && !IsDerivedModel && !IsReferenceType;
            EnumValues = IsEnum ? GetEnumValues(OpenApiSchema.Enum).ToList() : null;
            MaxLength = openApiSchema.MaxLength;
            MinLength = openApiSchema.MinLength;
        }

        protected virtual void ResolveType(OpenApiSchema openApiSchema)
        {
            if (openApiSchema.Type == null)
            {
                Type = typeof(object);

                return;
            }

            var openApiType = openApiSchema.Type;

            switch (openApiType)
            {
                case OpenApiSchemaTypes.Object:
                    {
                        ResolveObject(openApiSchema);

                        break;
                    }
                case OpenApiSchemaTypes.Integer:
                    {
                        ResolveInteger(openApiSchema);

                        break;
                    }
                case OpenApiSchemaTypes.String:
                    {
                        ResolveString(openApiSchema);

                        break;
                    }
                case OpenApiSchemaTypes.Array:
                    {
                        ResolveArray(openApiSchema);

                        break;
                    }
                case OpenApiSchemaTypes.Boolean:
                    {
                        ResolveBoolean(openApiSchema);

                        break;
                    }
                case OpenApiSchemaTypes.Number:
                    {
                        ResolveNumber(openApiSchema);

                        break;
                    }
            }
        }

        protected virtual string GetTypeFullName()
        {
            if (IsReferenceType)
            {
                return ReferenceModel.GetTypeFullName();
            }

            if (IsNullable)
            {
                return $"{Type.Namespace}.Nullable<{Type?.FullName}>";
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

        private void ResolveArray(OpenApiSchema openApiSchema)
        {
            var arrayItemType = new ApiModel(openApiSchema.Items, ApiModels, ApiFileName);

            if (string.IsNullOrWhiteSpace(Name))
            {
                var typeName = arrayItemType.IsBuiltInType ? arrayItemType.TypeName : arrayItemType.Name ?? arrayItemType.ReferenceModel?.Name;
                var name = openApiSchema.Description ?? $"{typeName}s";
                Name = name;
            }

            InnerTypes = new List<ApiModel>
            {
                arrayItemType
            };

            Type = typeof(IEnumerable<>);
        }

        private void ResolveString(OpenApiSchema openApiSchema)
        {
            if (string.IsNullOrWhiteSpace(openApiSchema.Format))
            {
                ResolveTypeAndDefaultValue<string>(openApiSchema.Default);
            }

            switch (openApiSchema.Format)
            {
                case OpenApiTypeFormats.Date:
                case OpenApiTypeFormats.DateTime:
                {
                    ResolveDateTime(openApiSchema);

                    break;
                }
                case OpenApiTypeFormats.Email:
                    {
                        IsEmail = true;
                        ResolveTypeAndDefaultValue<string>(openApiSchema.Default);

                        break;
                    }
                default:
                    {
                        ResolveTypeAndDefaultValue<string>(openApiSchema.Default);

                        break;
                    }
            }
        }

        private void ResolveDateTime(OpenApiSchema openApiSchema)
        {
            IsNullable = openApiSchema.Nullable;

            ResolveTypeAndDefaultValue<DateTime>(openApiSchema.Default);
        }

        private void ResolveInteger(OpenApiSchema openApiSchema)
        {
            IsNullable = openApiSchema.Nullable;

            if (string.IsNullOrWhiteSpace(openApiSchema.Format))
            {
                ResolveTypeAndDefaultValue<int>(openApiSchema.Default);
            }

            switch (openApiSchema.Format)
            {
                case OpenApiTypeFormats.Int32:
                    {
                        ResolveTypeAndDefaultValue<int>(openApiSchema.Default);

                        break;
                    }
                case OpenApiTypeFormats.Int64:
                    {
                        ResolveTypeAndDefaultValue<long>(openApiSchema.Default);

                        break;
                    }
                default:
                    {
                        ResolveTypeAndDefaultValue<int>(openApiSchema.Default);

                        break;
                    }
            }
        }

        private void ResolveObject(OpenApiSchema openApiSchema)
        {
            if (openApiSchema.AdditionalProperties == null || !openApiSchema.AdditionalPropertiesAllowed)
            {
                Type = typeof(object);
            }
            else
            {
                ResolveDictionaryType(openApiSchema.AdditionalProperties);
            }
        }

        private void ResolveBoolean(OpenApiSchema openApiSchema)
        {
            IsNullable = openApiSchema.Nullable;

            ResolveTypeAndDefaultValue<bool>(openApiSchema.Default);
        }

        private void ResolveTypeAndDefaultValue<T>(IOpenApiAny openApiSchema)
        {
            Type = typeof(T);
            if (openApiSchema is not OpenApiPrimitive<T> @default)
            {
                return;
            }

            DefaultValue = @default.Value;
        }

        private void ResolveDictionaryType(OpenApiSchema additionalProperties)
        {
            //TODO resolve reference types
            var dictArgType1 = new ApiModel(new OpenApiSchema { Type = OpenApiSchemaTypes.String }, ApiModels, ApiFileName);
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

        private static IEnumerable<string> GetEnumValues(IEnumerable<IOpenApiAny> enumValues)
        {
            foreach (var openApiAny in enumValues)
            {
                var enumValue = (OpenApiString)openApiAny;

                yield return enumValue.Value;
            }
        }

        private void ResolveNumber(OpenApiSchema openApiSchema)
        {
            IsNullable = openApiSchema.Nullable;

            if (string.IsNullOrWhiteSpace(openApiSchema.Format))
            {
                ResolveTypeAndDefaultValue<double>(openApiSchema.Default);
            }

            switch (openApiSchema.Format)
            {
                case OpenApiTypeFormats.Double:
                {
                    ResolveTypeAndDefaultValue<double>(openApiSchema.Default);

                    break;
                }
                case OpenApiTypeFormats.Float:
                {
                    ResolveTypeAndDefaultValue<float>(openApiSchema.Default);

                    break;
                }
                default:
                {
                    ResolveTypeAndDefaultValue<double>(openApiSchema.Default);

                    break;
                }
            }
        }
    }
}
