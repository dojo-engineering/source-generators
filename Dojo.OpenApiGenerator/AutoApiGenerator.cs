using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Dojo.Generators.Core.Utils;
using Dojo.OpenApiGenerator.CodeTemplates;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Stubble.Core;
using Stubble.Core.Builders;

namespace Dojo.OpenApiGenerator
{
    [Generator]
    public class AutoApiGenerator : ISourceGenerator
    {
        private string _serviceInterfaceTemplateString;
        private readonly StubbleVisitorRenderer _stubbleBuilder;
        private string _controllerTemplateString;
        private string _modelTemplateString;
        private Dictionary<string, ApiModel> _apiModels;
        private string _enumTemplateString;
        private string _projectDir;
        private AutoApiGeneratorSettings _autoApiGeneratorSettings;

        public AutoApiGenerator()
        {
            _stubbleBuilder = new StubbleBuilder().Build();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
            //#endif

            //Debug.WriteLine("Initialize code generator");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            _projectDir = context.GetProjectDir();
            _autoApiGeneratorSettings = AutoApiGeneratorSettings.GetAutoApiGeneratorSettings(_projectDir);

            var apisToOverride = new List<string>();

            GenerateApisSourceCode(context, apisToOverride);
        }

        private void GenerateApisSourceCode(GeneratorExecutionContext context, ICollection<string> apisToOverride)
        {
            var openApiDocuments = GetOpenApiDocuments(_projectDir);
            var projectNamespace = context.GetProjectDefaultNamespace();

            GenerateApiModelsSourceCode(context, openApiDocuments, projectNamespace);

            foreach (var openApiDocument in openApiDocuments)
            {
                GenerateApiSourceCode(context, apisToOverride, openApiDocument.Value, projectNamespace, openApiDocument.Key, _autoApiGeneratorSettings);
            }
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
            string apiFileName,
            AutoApiGeneratorSettings apiGeneratorSettings)
        {
            var apiVersion = openApiDocument.Info.Version;
            var parameters = openApiDocument.Components.Parameters.GetApiParameters(projectNamespace, _apiModels, apiVersion, apiFileName);
            var data = new ApiControllerDefinition
            {
                ProjectNamespace = projectNamespace,
                Title = openApiDocument.Info.Title,
                Version = apiVersion,
                SourceCodeVersion = GetSourceCodeVersion(apiVersion),
                Routes = openApiDocument.Paths.Select(x => new ApiControllerRoute(x.Key, x.Value, _apiModels, projectNamespace, parameters, apiVersion, apiFileName, apiGeneratorSettings)),
                CanOverride = apisToOverride.Contains(openApiDocument.Info.Title),
                Parameters = parameters
            };

            //GenerateServiceInterface(context, data);

            if (data.Routes.Any(x => x.Actions.Any()))
            {
                GenerateController(context, data, apiFileName);
            }
        }

        private static string GetSourceCodeVersion(string version)
        {
            return version.ToSourceCodeName();
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
            var openApiSchemasDir = $"{projectDir}\\{Constants.OpenApiSchemasFolder}";
            var schemaFiles = FileSystemUtils.FindFilesWithExtensions(openApiSchemasDir, Constants.OpenApiFileJsonExtension, Constants.OpenApiFileYamlExtension);

            foreach (var schemaFile in schemaFiles)
            {
                var file = new FileInfo(schemaFile);
                var schema = File.ReadAllText(schemaFile);
                var openApiReader = new OpenApiStringReader();
                var openApiDocument = openApiReader.Read(schema, out _);

                openApiDocuments.Add(file.Name, openApiDocument);
            }

            return openApiDocuments;
        }
    }
}
