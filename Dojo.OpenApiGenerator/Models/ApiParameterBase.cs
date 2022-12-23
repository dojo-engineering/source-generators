﻿using System.Collections.Generic;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class ApiParameterBase : IHasApiConstraints
    {
        public string Version { get; }
        public IDictionary<string, ApiModel> ApiModels { get; }
        public string ApiFileName { get; }
        public AutoApiGeneratorSettings AutoApiGeneratorSettings { get; }
        public string Name { get; set; }
        public ApiModel ApiModel { get; set; }
        public bool IsRequired { get; }
        public int? MaxLength { get; }
        public int? MinLength { get; }
        public abstract ParameterLocation ParameterLocation { get; }
        public string SourceCodeName { get; }

        protected readonly OpenApiParameter OpenApiParameter;

        protected ApiParameterBase(
            string sourceCodeName, 
            OpenApiParameter openApiParameter,
            string apiVersion,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName,
            AutoApiGeneratorSettings autoApiGeneratorSettings)
        {
            Version = apiVersion;
            ApiModels = apiModels;
            ApiFileName = apiFileName;
            AutoApiGeneratorSettings = autoApiGeneratorSettings;
            OpenApiParameter = openApiParameter;
            Name = openApiParameter.Name;
            ApiModel = ResolveApiModel();
            IsRequired = openApiParameter.Required;
            SourceCodeName = sourceCodeName ?? Name.ToSourceCodeParameterName();
            MaxLength = openApiParameter.Schema?.MaxLength;
            MinLength = openApiParameter.Schema?.MinLength;
        }

        protected abstract ApiModel ResolveApiModel();
    }
}
