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
using Stubble.Core.Builders;

namespace Dojo.OpenApiGenerator
{
    [Generator]
    public class AutoApiGenerator : ISourceGenerator
    {
        private const string OpenApiFileExtension = ".json";
        private const string ControllerOverrideAttributeName = "AutoControllerOverride";

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

        private static void GenerateApiSourceCode(GeneratorExecutionContext context, ICollection<string> apisToOverride, OpenApiDocument openApiDocument)
        {
            var projectNamespace = context.GetProjectDefaultNamespace();
            var controllerTemplateString = Templates.ReadTemplate(Templates.Controller);
            var modelTemplateString = Templates.ReadTemplate(Templates.Model);
            var serviceInterfaceTemplateString = Templates.ReadTemplate(Templates.ServiceInterface);
            var stubbleBuilder = new StubbleBuilder().Build();
            var apiModels = GenerateApiModels(openApiDocument, projectNamespace);
            var data = new ApiControllerDefinition
            {
                ProjectNamespace = projectNamespace,
                Title = openApiDocument.Info.Title,
                Version = openApiDocument.Info.Version,
                Routes = openApiDocument.Paths.Select(x => ApiControllerRoute.Create(x.Key, x.Value, apiModels)),
                Models = apiModels,
                CanOverride = apisToOverride.Contains(openApiDocument.Info.Title)
            };

            var controllerSourceCode = stubbleBuilder.Render(controllerTemplateString, data);
            var serviceInterfaceSourceCode = stubbleBuilder.Render(serviceInterfaceTemplateString, data);

            context.AddSource($"{data.Title}Controller.g.cs", SourceText.From(controllerSourceCode, Encoding.UTF8));
            context.AddSource($"I{data.Title}Service.g.cs", SourceText.From(serviceInterfaceSourceCode, Encoding.UTF8));

            foreach (var x in data.Models)
            {
                var name = x.Key;
                var apiModel = x.Value;
                var modelSource = stubbleBuilder.Render(modelTemplateString, apiModel);
                context.AddSource($"{name}ApiModel.g.cs", SourceText.From(modelSource, Encoding.UTF8));
            }
        }

        private static Dictionary<string, ApiModel> GenerateApiModels(OpenApiDocument openApiDocument, string projectNamespace)
        {
            var apiModels =
                openApiDocument.Components.Schemas.Select(x => ApiModel.Create(x.Value, projectNamespace));

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
