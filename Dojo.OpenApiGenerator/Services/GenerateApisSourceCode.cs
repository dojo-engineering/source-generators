using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Dojo.Generators.Core.Utils;
using Dojo.OpenApiGenerator;
using Dojo.OpenApiGenerator.CodeTemplates;
using Dojo.OpenApiGenerator.Configuration;
using Dojo.OpenApiGenerator.Extensions;
using Dojo.OpenApiGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Stubble.Core;

public class GenerateApisSourceCode : IGenerateApisSourceCode
{
    private static readonly HashSet<string> ApiVersions = new();
    private Dictionary<string, ApiModel> _apiModels;
    private AutoApiGeneratorSettings _autoApiGeneratorSettings;
    private string? _modelTemplateString;
    private string? _enumTemplateString;
    private string? _controllerTemplateString;
    private string? _apiConstantsTemplateString;
    public GenerateApisSourceCode(){}

    public Result GenerateApiSourceCode(GeneratorExecutionContext context, 
    string projectNamespace, 
    StubbleVisitorRenderer stubbleBuilder, 
    ICollection<string> apisToOverride, 
    string projectDir, 
    AutoApiGeneratorSettings autoApiGeneratorSettings)
    {   
        var openApiDocuments = GetOpenApiDocuments(projectDir);
        _autoApiGeneratorSettings = autoApiGeneratorSettings;

        GenerateApiModelsSourceCode(context, openApiDocuments, projectNamespace, stubbleBuilder);

        foreach (var openApiDocument in openApiDocuments)
        {
            GenerateApiSourceCodes(context, apisToOverride, openApiDocument.Value, projectNamespace, openApiDocument.Key, stubbleBuilder);
        }

        GenerateApiVersionsSourceCode(context, projectNamespace, stubbleBuilder);
        return Result.Success();
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

    private void GenerateApiModelsSourceCode(GeneratorExecutionContext context, IDictionary<string, OpenApiDocument> openApiDocuments, string projectNamespace, StubbleVisitorRenderer stubbleBuilder)
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

        GenerateApiModelsSources(context, _apiModels, stubbleBuilder);
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

    private void GenerateApiModelsSources(GeneratorExecutionContext context, IDictionary<string, ApiModel> apiModels, StubbleVisitorRenderer stubbleBuilder)
    {
        foreach (var x in apiModels)
        {
            var apiModel = x.Value;
            var name = x.Key;

            if (apiModel.IsEnum)
            {
                GenerateEnum(context, apiModel, name, stubbleBuilder);
            }
            else
            {
                GenerateApiModel(context, apiModel, name, stubbleBuilder);
            }
        }
    }

    private static Dictionary<string, ApiModel> GetApiModels(OpenApiDocument openApiDocument, string projectNamespace, string apiVersion, IDictionary<string, ApiModel> apiModels, string apiFileName)
    {
        var models =
            openApiDocument.Components.Schemas.Select(x => new ApiModel(x.Key, x.Value, projectNamespace, apiVersion, apiModels, apiFileName));

        return models.ToDictionary(x => x.Name);
    }

    private void GenerateApiSourceCodes(
        GeneratorExecutionContext context,
        ICollection<string> apisToOverride,
        OpenApiDocument openApiDocument,
        string projectNamespace,
        string apiFileName, 
        StubbleVisitorRenderer stubbleBuilder)
    {
        var apiVersion = openApiDocument.Info.Version;
        var supportedApiVersions = TryGetSupportedApiVersions(openApiDocument) ?? new HashSet<string>();
        if (!string.IsNullOrWhiteSpace(apiVersion))
        {
            supportedApiVersions.Add(apiVersion);
        }

        var parameters = openApiDocument.Components.Parameters.GetApiParameters(projectNamespace, _apiModels, apiVersion, apiFileName);
        var data = new ApiControllerDefinition(projectNamespace)
        {
            Title = openApiDocument.Info.Title,
            SupportedVersions = supportedApiVersions,
            SourceCodeVersion = StringHelpers.ToSourceCodeVersion(supportedApiVersions, apiVersion),
            Routes = openApiDocument.Paths.Select(x => new ApiControllerRoute(x.Key, x.Value, _apiModels, projectNamespace, parameters, apiVersion, apiFileName, _autoApiGeneratorSettings)),
            CanOverride = apisToOverride.Contains(openApiDocument.Info.Title),
            Parameters = parameters,
            AuthorizationPolicies = openApiDocument.TryGetApiAuthorizationPolicies(_autoApiGeneratorSettings.ApiAuthorizationPoliciesExtension)
        };

        foreach (var supportedApiVersion in supportedApiVersions)
        {
            ApiVersions.Add(supportedApiVersion);
        }

        //GenerateServiceInterface(context, data);

        if (data.Routes.Any(x => x.Actions.Any()))
        {
            GenerateController(context, data, apiFileName, stubbleBuilder);
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
            }
        }

        return supportedVersions.Any() ? supportedVersions : null;
    }

   

    private void GenerateApiVersionsSourceCode(GeneratorExecutionContext context, string projectNamespace, StubbleVisitorRenderer stubbleBuilder)
    {
        _apiConstantsTemplateString ??= Templates.ReadTemplate(Templates.ApiConstantsTemplate);

        const string fileName = "ApiConstants.g.cs";
        var source = stubbleBuilder.Render(_apiConstantsTemplateString, new ApiConstants(ApiVersions.ToList(), projectNamespace));

        context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
    }


    private void GenerateApiModel(GeneratorExecutionContext context, ApiModel apiModel, string name, StubbleVisitorRenderer stubbleBuilder)
    {
        _modelTemplateString ??= Templates.ReadTemplate(Templates.Model);

        var fileName = $"{name}ApiModel.g.cs";
        var modelSource = stubbleBuilder.Render(_modelTemplateString, apiModel);

        context.AddSource(fileName, SourceText.From(modelSource, Encoding.UTF8));
    }

    private void GenerateEnum(GeneratorExecutionContext context, ApiModel apiModel, string name, StubbleVisitorRenderer stubbleBuilder)
    {
        _enumTemplateString ??= Templates.ReadTemplate(Templates.Enum);

        var fileName = $"{name}.g.cs";
        var enumSource = stubbleBuilder.Render(_enumTemplateString, apiModel);

        context.AddSource(fileName, SourceText.From(enumSource, Encoding.UTF8));
    }

    private void GenerateController(GeneratorExecutionContext context, ApiControllerDefinition data, string apiFileName, StubbleVisitorRenderer stubbleBuilder)
    {
        _controllerTemplateString ??= Templates.ReadTemplate(Templates.AbstractController);

        var fileName = $"{apiFileName}_{data.Title}ControllerBase.g.cs";
        var controllerSourceCode = stubbleBuilder.Render(_controllerTemplateString, data);

        context.AddSource(fileName, SourceText.From(controllerSourceCode, Encoding.UTF8));
    }

    // private void GenerateServiceInterface(GeneratorExecutionContext context, ApiControllerDefinition data)
    // {
    //     _serviceInterfaceTemplateString ??= Templates.ReadTemplate(Templates.ServiceInterface);

    //     var fileName = $"I{data.Title}Service.g.cs";
    //     var serviceInterfaceSourceCode = _stubbleBuilder.Render(_serviceInterfaceTemplateString, data);

    //     context.AddSource(fileName, SourceText.From(serviceInterfaceSourceCode, Encoding.UTF8));
    // }
}
