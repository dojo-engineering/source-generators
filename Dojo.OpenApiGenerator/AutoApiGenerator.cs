using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dojo.OpenApiGenerator.CodeTemplates;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Models;
using Dojo.OpenApiGenerator.Utils;
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
            GenerateApiSourceCode(context);
        }

        private void GenerateApiSourceCode(GeneratorExecutionContext context)
        {
            var projectDir = context.GetProjectDir();
            var projectNamespace = context.GetProjectDefaultNamespace();
            var controllerTemplateString = Templates.ReadTemplate(Templates.Controller);
            var modelTemplateString = Templates.ReadTemplate(Templates.Model);
            var stubbleBuilder = new StubbleBuilder().Build();
            var openApiDocument = GetOpenApiDocument(projectDir, "hello-source-generators-api.json");
            var apiModels = GenerateApiModels(openApiDocument, projectNamespace);
            var data = new ApiControllerDefinition
            {
                ProjectNamespace = projectNamespace,
                Title = openApiDocument.Info.Title,
                Version = openApiDocument.Info.Version,
                Paths = openApiDocument.Paths.Select(x => ApiControllerRoute.Create(x.Key, x.Value, apiModels)),
                Models = apiModels
            };

            var controllerSourceCode = stubbleBuilder.Render(controllerTemplateString, data);

            context.AddSource($"{data.Title}Controller.g.cs", SourceText.From(controllerSourceCode, Encoding.UTF8));

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
                openApiDocument.Components.Schemas.Select(x => ApiModel.Create(x.Key, x.Value, projectNamespace));

            return apiModels.ToDictionary(x => x.Name);
        }

        private OpenApiDocument GetOpenApiDocument(string projectDir, string schemaFileName)
        {
            var openApiSchemasDir = $"{projectDir}\\OpenApiSchemas";
            var schemaFile = FileSystemUtil.FindFile(openApiSchemasDir, schemaFileName);
            var schema = File.ReadAllText(schemaFile);
            var openApiReader = new OpenApiStringReader();

            return openApiReader.Read(schema, out _);
        }
    }
}
