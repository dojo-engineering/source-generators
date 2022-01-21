using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Dojo.Generators.Core.CodeAnalysis;
using Dojo.Generators.Core.Utils;
using Dojo.OpenApiGenerator.CodeTemplates;
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
        private const string OpenApiFileExtension = ".json";
        private const string ControllerOverrideAttributeName = "AutoControllerOverride";

        private static string _serviceInterfaceTemplateString;
        private static readonly StubbleVisitorRenderer StubbleBuilder;
        private static string _controllerTemplateString;
        private static string _modelTemplateString;

        static AutoApiGenerator()
        {
            StubbleBuilder = new StubbleBuilder().Build();
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
            context.RegisterForSyntaxNotifications(() => new ClassWithAttributeSyntaxReceiver(ControllerOverrideAttributeName));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var apisToOverride = new List<string>();
            var syntaxReceiver = context.SyntaxReceiver as ClassWithAttributeSyntaxReceiver;
            foreach (var candidateClass in syntaxReceiver.CandidateClasses)
            {
                var autoControllerOverrideRoute = candidateClass.AttributeLists.Select(a => a.Attributes.First(x => x.Name.ToFullString() == ControllerOverrideAttributeName)).First();
                apisToOverride.Add(autoControllerOverrideRoute.ArgumentList.Arguments.First().ToFullString().Trim('"'));
            }

            GenerateApisSourceCode(context, apisToOverride);
        }

        private void GenerateApisSourceCode(GeneratorExecutionContext context, ICollection<string> apisToOverride)
        {
            var projectDir = context.GetProjectDir();
            var openApiDocuments = GetOpenApiDocuments(projectDir);

            foreach (var openApiDocument in openApiDocuments)
            {
                GenerateApiSourceCode(context, apisToOverride, openApiDocument);
            }
        }

        private static void GenerateApiSourceCode(
            GeneratorExecutionContext context,
            ICollection<string> apisToOverride,
            OpenApiDocument openApiDocument)
        {
            var projectNamespace = context.GetProjectDefaultNamespace();
            var apiModels = GenerateApiModels(openApiDocument, projectNamespace);
            var parameters = openApiDocument.Components.Parameters.GetApiParameters(projectNamespace, apiModels);
            var data = new ApiControllerDefinition
            {
                ProjectNamespace = projectNamespace,
                Title = openApiDocument.Info.Title,
                Version = openApiDocument.Info.Version,
                Routes = openApiDocument.Paths.Select(x => new ApiControllerRoute(x.Key, x.Value, apiModels, projectNamespace, parameters)),
                Models = apiModels,
                CanOverride = apisToOverride.Contains(openApiDocument.Info.Title),
                Parameters = parameters
            };

            GenerateModels(context, data);
            //GenerateServiceInterface(context, data);
            GenerateController(context, data);
        }

        private static void GenerateModels(GeneratorExecutionContext context, ApiControllerDefinition data)
        {
            _modelTemplateString ??= Templates.ReadTemplate(Templates.Model);

            foreach (var x in data.Models)
            {
                var name = x.Key;
                var apiModel = x.Value;
                var modelSource = StubbleBuilder.Render(_modelTemplateString, apiModel);

                context.AddSource($"{name}ApiModel.g.cs", SourceText.From(modelSource, Encoding.UTF8));
            }
        }

        private static void GenerateController(GeneratorExecutionContext context, ApiControllerDefinition data)
        {
            _controllerTemplateString ??= Templates.ReadTemplate(Templates.AbstractController);

            var controllerSourceCode = StubbleBuilder.Render(_controllerTemplateString, data);

            context.AddSource($"{data.Title}ControllerBase.g.cs", SourceText.From(controllerSourceCode, Encoding.UTF8));
        }

        private static void GenerateServiceInterface(GeneratorExecutionContext context, ApiControllerDefinition data)
        {
            _serviceInterfaceTemplateString ??= Templates.ReadTemplate(Templates.ServiceInterface);

            var serviceInterfaceSourceCode = StubbleBuilder.Render(_serviceInterfaceTemplateString, data);

            context.AddSource($"I{data.Title}Service.g.cs", SourceText.From(serviceInterfaceSourceCode, Encoding.UTF8));
        }

        private static Dictionary<string, ApiModel> GenerateApiModels(OpenApiDocument openApiDocument, string projectNamespace)
        {
            var apiModels =
                openApiDocument.Components.Schemas.Select(x => new ApiModel(x.Key, x.Value, projectNamespace));

            return apiModels.ToDictionary(x => x.Name);
        }

        private IEnumerable<OpenApiDocument> GetOpenApiDocuments(string projectDir)
        {
            var openApiSchemasDir = $"{projectDir}\\{Constants.OpenApiSchemasFolder}";
            var schemaFiles = FileSystemUtils.FindFilesWithExtension(openApiSchemasDir, OpenApiFileExtension);

            foreach (var schemaFile in schemaFiles)
            {
                var schema = File.ReadAllText(schemaFile);
                var openApiReader = new OpenApiStringReader();

                yield return openApiReader.Read(schema, out _);
            }
        }
    }
}
