using System;
using System.Collections.Generic;
using System.Linq;
using Dojo.OpenApiGenerator.Configuration;
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
        public AutoApiGeneratorSettings AutoApiGeneratorSettings { get; }
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
        public object DefaultValue { get; private set; }
        public bool IsNullable { get; private set; }
        public bool IsUri { get; private set; }
        public int? MaxLength { get; }
        public int? MinLength { get; }

        protected ApiModelBase(
            OpenApiSchema openApiSchema,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            string projectNamespace,
            AutoApiGeneratorSettings autoApiGeneratorSettings) : base(projectNamespace)
        {
            if (openApiSchema.OneOf != null && openApiSchema.OneOf.Any())
            {
                OpenApiSchema = openApiSchema.OneOf.FirstOrDefault();
            }
            else if (openApiSchema.AnyOf != null && openApiSchema.AnyOf.Any())
            {
                OpenApiSchema =  openApiSchema.AnyOf.FirstOrDefault();

                var nullableType = openApiSchema.AnyOf.ElementAt(1);
                IsNullable = nullableType is { Nullable: true };
            }
            else
            {
                OpenApiSchema = openApiSchema;
            }

            ApiModels = apiModels;
            ApiFileName = apiFileName;
            AutoApiGeneratorSettings = autoApiGeneratorSettings;
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
            }
        }

        protected virtual string GetTypeFullName()
        {
            var typeFullName = IsReferenceType ? ReferenceModel.GetTypeFullName() : Type?.FullName;

            if (IsReferenceType)
            {
                return typeFullName;
            }

            if (IsNullable && !IsUri && Type != typeof(string))
            {
                return $"System.Nullable<{typeFullName}>";
            }

            if (Type == typeof(IDictionary<,>))
            {
                return $"System.Collections.Generic.IDictionary<{InnerTypes[0].TypeFullName},{InnerTypes[1].TypeFullName}>";
            }

            if (Type == typeof(IEnumerable<>))
            {
                return $"System.Collections.Generic.IEnumerable<{InnerTypes[0].TypeFullName}>";
            }

            return Type?.FullName;
        }

        private void ResolveArray(OpenApiSchema openApiSchema)
        {
            var arrayItemType = new ApiModel(openApiSchema.Items, ApiModels, ApiFileName, AutoApiGeneratorSettings);

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
                case OpenApiTypeFormats.Uri:
                    {
                        IsUri = true;
                        ResolveUri(openApiSchema);

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

        private void ResolveUri(OpenApiSchema openApiSchema)
        {
            IsNullable = openApiSchema.Nullable;

            Type = typeof(Uri);
            if (openApiSchema.Default is not OpenApiString @default)
            {
                return;
            }

            DefaultValue = new Uri(@default.Value);
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
            var dictArgType1 = new ApiModel(new OpenApiSchema { Type = OpenApiSchemaTypes.String }, ApiModels, ApiFileName, AutoApiGeneratorSettings);
            var dictArgType2 = new ApiModel(additionalProperties, ApiModels, ApiFileName, AutoApiGeneratorSettings);

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
    }
}