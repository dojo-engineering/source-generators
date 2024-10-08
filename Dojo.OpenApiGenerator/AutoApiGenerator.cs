﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Dojo.Generators.Core.Utils;
using Dojo.OpenApiGenerator.CodeTemplates;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Exceptions;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Stubble.Core;
using Stubble.Core.Builders;

namespace Dojo.OpenApiGenerator
{
    [Generator]
    public class AutoApiGenerator : ISourceGenerator
    {
        private static readonly HashSet<string> ApiVersions = new();

        private readonly StubbleVisitorRenderer _stubbleBuilder;
        private string _projectDir;
        private string _serviceInterfaceTemplateString;
        private string _apiConstantsTemplateString;
        private string _controllerTemplateString;
        private string _modelTemplateString;
        private string _enumTemplateString;
        private string _inheritedApiVersionAttributeTemplateString;
        private Dictionary<string, ApiModel> _apiModels;
        private AutoApiGeneratorSettings _autoApiGeneratorSettings;
        private string _apiConfiguratorTemplateString;
        private string _apiVersionsTemplateString;
        private string _dictionaryModelTemplate;

        public AutoApiGenerator()
        {
            _stubbleBuilder = new StubbleBuilder().Build();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif
            Debug.WriteLine("Initialize code generator");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                _projectDir = context.GetProjectDir();
                _autoApiGeneratorSettings = AutoApiGeneratorSettings.GetAutoApiGeneratorSettings(_projectDir);
                var projectNamespace = context.GetProjectDefaultNamespace();
                var apisToOverride = new List<string>();

                GenerateApiConfiguratorSourceCode(context, projectNamespace, _autoApiGeneratorSettings.GenerateApiExplorer);
                GenerateInheritedApiVersionAttributeSourceCode(context, projectNamespace);
                GenerateApisSourceCode(context, apisToOverride, projectNamespace);  
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine($"Ignore auto api generator for directory {_projectDir} because of exception: {exception.Message}");
            }
        }

        private void GenerateApisSourceCode(GeneratorExecutionContext context, ICollection<string> apisToOverride, string projectNamespace)
        {
            var openApiDocuments = GetOpenApiDocuments(_projectDir);

            GenerateApiModelsSourceCode(context, openApiDocuments, projectNamespace);

            foreach (var openApiDocument in openApiDocuments)
            {
                GenerateApiSourceCode(context, apisToOverride, openApiDocument.Value, projectNamespace, openApiDocument.Key);
            }

            GenerateApiConstantsSourceCode(context, projectNamespace);
            GenerateApiVersionsSourceCode(context, projectNamespace);
        }

        private void GenerateApiConfiguratorSourceCode(GeneratorExecutionContext context, string projectNamespace, bool generateApiExplorer)
        {
            _apiConfiguratorTemplateString ??= Templates.ReadTemplate(Templates.ApiConfiguratorTemplate);

            const string fileName = "ApiConfigurator.g.cs";
            var source = _stubbleBuilder.Render(_apiConfiguratorTemplateString, new BasicClass(projectNamespace, generateApiExplorer));

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }

        private void GenerateInheritedApiVersionAttributeSourceCode(GeneratorExecutionContext context, string projectNamespace)
        {
            _inheritedApiVersionAttributeTemplateString ??= Templates.ReadTemplate(Templates.InheritedApiVersionAttribute);

            const string fileName = "InheritedApiVersionAttribute.g.cs";
            var source = _stubbleBuilder.Render(_inheritedApiVersionAttributeTemplateString, new BasicClass(projectNamespace));

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }

        private void GenerateApiConstantsSourceCode(GeneratorExecutionContext context, string projectNamespace)
        {
            _apiConstantsTemplateString ??= Templates.ReadTemplate(Templates.ApiConstantsTemplate);

            const string fileName = "ApiConstants.g.cs";
            var source = _stubbleBuilder.Render(_apiConstantsTemplateString, new ApiConstants(ApiVersions.ToList(), projectNamespace));

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }

        private void GenerateApiVersionsSourceCode(GeneratorExecutionContext context, string projectNamespace)
        {
            _apiVersionsTemplateString ??= Templates.ReadTemplate(Templates.ApiVersionsTemplate);

            const string fileName = "ApiVersions.g.cs";
            var source = _stubbleBuilder.Render(_apiVersionsTemplateString, new ApiConstants(ApiVersions.ToList(), projectNamespace));

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }

        private void GenerateApiModelsSourceCode
        (
            GeneratorExecutionContext context,
            IDictionary<string, OpenApiDocument> openApiDocuments,
            string projectNamespace)
        {
            _apiModels = new Dictionary<string, ApiModel>();
            var shared = openApiDocuments.FirstOrDefault(x => x.Value.Info.Title == Constants.SharedModelsTitle);

            if (shared.Value != null)
            {
                BuildApiModels(shared.Value, projectNamespace, _apiModels, shared.Key);

                openApiDocuments.Remove(shared);
            }

            foreach (var openApiDocument in openApiDocuments)
            {
                BuildApiModels(openApiDocument.Value, projectNamespace, _apiModels, openApiDocument.Key);
            }

            GenerateApiModelsSources(context, _apiModels);
        }

        private void GenerateApiSourceCode(
            GeneratorExecutionContext context,
            ICollection<string> apisToOverride,
            OpenApiDocument openApiDocument,
            string projectNamespace,
            string apiFileName)
        {
            const string defaultTag = "defaultTag";
            var apiVersion = openApiDocument.Info.Version;
            var controllerDefinitions = new List<ApiControllerDefinition>();
            var operationsByTags = new Dictionary<string, Dictionary<string, (OpenApiPathItem Path, IDictionary<OperationType, OpenApiOperation> Operations)>>();

            var supportedApiVersions = TryGetSupportedApiVersions(openApiDocument) ?? new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(apiVersion))
            {
                supportedApiVersions.Add(apiVersion);
            }

            var parameters = openApiDocument.Components.Parameters.GetApiParameters(projectNamespace, _apiModels, apiVersion, apiFileName);

            if (_autoApiGeneratorSettings.OrganizeControllersByTags)
            {
                foreach (var openApiPath in openApiDocument.Paths)
                {
                    if (openApiPath.Value?.Operations == null)
                    {
                        throw new InvalidOpenApiSchemaException($"No Operations defined for path: '{openApiPath.Key}'");
                    }

                    foreach (var openApiOperation in openApiPath.Value.Operations)
                    {
                        var tags = openApiOperation.Value.Tags;

                        if (tags == null || tags.Count == 0)
                        {
                            tags = new List<OpenApiTag> { new() { Name = defaultTag } };
                        }

                        foreach (var tag in tags)
                        {
                            if (!operationsByTags.TryGetValue(tag.Name, out var operationsByTag))
                            {
                                operationsByTag = new Dictionary<string, (OpenApiPathItem Path, IDictionary<OperationType, OpenApiOperation> Operations)>();
                                operationsByTags.Add(tag.Name, operationsByTag);
                            }

                            if (!operationsByTag.TryGetValue(openApiPath.Key, out var operationPathByTag))
                            {
                                operationPathByTag = (openApiPath.Value, new Dictionary<OperationType, OpenApiOperation>());
                                operationsByTag.Add(openApiPath.Key, operationPathByTag);
                            }

                            operationPathByTag.Operations.Add(openApiOperation);
                        }
                    }
                }

                controllerDefinitions.AddRange(operationsByTags.Select(operationsByTag => new ApiControllerDefinition(projectNamespace)
                {
                    Title = operationsByTag.Key == defaultTag ? openApiDocument.Info.Title.ToSourceCodeName(true) : operationsByTag.Key.ToSourceCodeName(true),
                    SupportedVersions = supportedApiVersions,
                    SourceCodeVersion = StringHelpers.ToSourceCodeVersion(supportedApiVersions, apiVersion),
                    Routes = operationsByTag.Value.Select(x => new ApiControllerRoute(x.Key, x.Value.Path, x.Value.Operations, _apiModels, projectNamespace, parameters, apiVersion, apiFileName, _autoApiGeneratorSettings)),
                    CanOverride = apisToOverride.Contains(openApiDocument.Info.Title),
                    Parameters = parameters,
                    AuthorizationPolicies = openApiDocument.TryGetApiAuthorizationPolicies(_autoApiGeneratorSettings.ApiAuthorizationPoliciesExtension)
                }));
            }
            else
            {
                controllerDefinitions.Add(new ApiControllerDefinition(projectNamespace)
                {
                    Title = openApiDocument.Info.Title.ToSourceCodeName(true),
                    SupportedVersions = supportedApiVersions,
                    SourceCodeVersion = StringHelpers.ToSourceCodeVersion(supportedApiVersions, apiVersion),
                    Routes = openApiDocument.Paths.Select(x => new ApiControllerRoute(x.Key, x.Value, x.Value.Operations, _apiModels, projectNamespace, parameters, apiVersion, apiFileName, _autoApiGeneratorSettings)),
                    CanOverride = apisToOverride.Contains(openApiDocument.Info.Title),
                    Parameters = parameters,
                    AuthorizationPolicies = openApiDocument.TryGetApiAuthorizationPolicies(_autoApiGeneratorSettings.ApiAuthorizationPoliciesExtension)
                });
            }

            foreach (var supportedApiVersion in supportedApiVersions)
            {
                ApiVersions.Add(supportedApiVersion);
            }

            //GenerateServiceInterface(context, data);

            foreach (var apiControllerDefinition in controllerDefinitions)
            {
                if (apiControllerDefinition.Routes.Any(x => x.Actions.Any()))
                {
                    GenerateController(context, apiControllerDefinition, apiFileName);
                }
                else
                {
                    throw new InvalidOpenApiSchemaException($"No Routes or Actions found for api controller definition: {apiControllerDefinition.Title}");
                }
            }
        }

        private HashSet<string> TryGetSupportedApiVersions(IOpenApiExtensible openApiDocument)
        {
            var supportedVersionExtensionName = _autoApiGeneratorSettings.OpenApiSupportedVersionsExtension;

            if (string.IsNullOrWhiteSpace(supportedVersionExtensionName))
            {
                return null;
            }

            if (!openApiDocument.Extensions.TryGetValue(supportedVersionExtensionName, out var extension))
            {
                return null;
            }

            if (extension is not OpenApiArray extensionValues || !extensionValues.Any())
            {
                return null;
            }

            var supportedVersions = new HashSet<string>();

            foreach (var extensionValue in extensionValues.Where(x => x != null))
            {
                switch (extensionValue)
                {
                    case OpenApiString openApiString:
                        {
                            if (string.IsNullOrWhiteSpace(openApiString.Value))
                            {
                                continue;
                            }

                            supportedVersions.Add(openApiString.Value);

                            break;
                        }
                    case OpenApiDateTime openApiDateTime:
                    {
                        supportedVersions.Add(openApiDateTime.Value.ToString(_autoApiGeneratorSettings.DateTimeVersionFormat));

                        break;
                    }
                    case OpenApiDate openApiDate:
                    {
                            supportedVersions.Add(openApiDate.Value.ToString(_autoApiGeneratorSettings.DateTimeVersionFormat));

                        break;
                    }
                }
            }

            return supportedVersions.Any() ? supportedVersions : null;
        }

        private static void BuildApiModels(OpenApiDocument openApiDocument,
            string projectNamespace,
            IDictionary<string, ApiModel> apiModels,
            string apiFileName)
        {
            var apiVersion = openApiDocument?.Info?.Version;
            var models = GetApiModels(openApiDocument, projectNamespace, apiVersion, apiModels, apiFileName);

            if (models == null || !models.Any())
            {
                return;
            }

            foreach (var x in models)
            {
                var key = x.Key.GetApiModelKey(apiFileName);

                if (apiModels.ContainsKey(key))
                {
                    continue;
                }

                apiModels.Add(key, x.Value);
            }
        }

        private void GenerateApiModelsSources(
            GeneratorExecutionContext context,
            IDictionary<string, ApiModel> apiModels)
        {
            foreach (var x in apiModels)
            {
                var apiModel = x.Value;
                var name = x.Key;

                if (apiModel.IsEnum)
                {
                    GenerateEnum(context, apiModel, name);
                }
                else if (apiModel.IsDictionary)
                {
                    GenerateDictionaryModel(context, apiModel, name);
                }
                else
                {
                    GenerateApiModel(context, apiModel, name);
                }
            }
        }

        private void GenerateApiModel(GeneratorExecutionContext context, ApiModel apiModel, string name)
        {
            _modelTemplateString ??= Templates.ReadTemplate(Templates.Model);

            var fileName = $"{name}ApiModel.g.cs";
            var modelSource = _stubbleBuilder.Render(_modelTemplateString, apiModel);

            context.AddSource(fileName, SourceText.From(modelSource, Encoding.UTF8));
        }

        private void GenerateEnum(GeneratorExecutionContext context, ApiModel apiModel, string name)
        {
            _enumTemplateString ??= Templates.ReadTemplate(Templates.Enum);

            var fileName = $"{name}.g.cs";
            var enumSource = _stubbleBuilder.Render(_enumTemplateString, apiModel);

            context.AddSource(fileName, SourceText.From(enumSource, Encoding.UTF8));
        }

        private void GenerateDictionaryModel(GeneratorExecutionContext context, ApiModel apiModel, string name)
        {
            _dictionaryModelTemplate ??= Templates.ReadTemplate(Templates.DictionaryModelTemplate);

            var fileName = $"{name}.g.cs";
            var dictionarySource = _stubbleBuilder.Render(_dictionaryModelTemplate, apiModel);

            context.AddSource(fileName, SourceText.From(dictionarySource, Encoding.UTF8));
        }

        private void GenerateController(GeneratorExecutionContext context, ApiControllerDefinition data, string apiFileName)
        {
            _controllerTemplateString ??= Templates.ReadTemplate(Templates.AbstractController);

            var fileName = $"{apiFileName}_{data.Title}ControllerBase.g.cs";
            var controllerSourceCode = _stubbleBuilder.Render(_controllerTemplateString, data);

            context.AddSource(fileName, SourceText.From(controllerSourceCode, Encoding.UTF8));
        }

        private void GenerateServiceInterface(GeneratorExecutionContext context, ApiControllerDefinition data)
        {
            _serviceInterfaceTemplateString ??= Templates.ReadTemplate(Templates.ServiceInterface);

            var fileName = $"I{data.Title}Service.g.cs";
            var serviceInterfaceSourceCode = _stubbleBuilder.Render(_serviceInterfaceTemplateString, data);

            context.AddSource(fileName, SourceText.From(serviceInterfaceSourceCode, Encoding.UTF8));
        }

        private static Dictionary<string, ApiModel> GetApiModels(OpenApiDocument openApiDocument, string projectNamespace, string apiVersion, IDictionary<string, ApiModel> apiModels, string apiFileName)
        {
            var models =
                openApiDocument.Components.Schemas.Select(x => new ApiModel(x.Key, x.Value, projectNamespace, apiVersion, apiModels, apiFileName));

            return models.ToDictionary(x => x.Name);
        }

        private IDictionary<string, OpenApiDocument> GetOpenApiDocuments(string projectDir)
        {
            var openApiDocuments = new Dictionary<string, OpenApiDocument>();
            var openApiSchemasDir = $"{projectDir}/{Constants.OpenApiSchemasFolder}";
            var schemaFiles = FileSystemUtils.FindFilesWithExtensions(openApiSchemasDir, Constants.OpenApiFileJsonExtension, Constants.OpenApiFileYamlExtension, Constants.OpenApiFileYmlExtension);
            var openApiReader = new OpenApiStringReader();

            foreach (var schemaFile in schemaFiles)
            {
                var file = new FileInfo(schemaFile);
                var schema = File.ReadAllText(schemaFile);
                var openApiDocument = openApiReader.Read(schema, out _);

                openApiDocuments.Add(file.Name, openApiDocument);
            }

            return openApiDocuments;
        }
    }
}
