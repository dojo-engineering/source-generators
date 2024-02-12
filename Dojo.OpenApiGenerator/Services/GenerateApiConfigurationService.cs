using System.Text;
using CSharpFunctionalExtensions;
using Dojo.OpenApiGenerator.CodeTemplates;
using Dojo.OpenApiGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Stubble.Core;

public class GenerateApiConfigurationService : IGenerateApiConfigurationService
{
    private string _apiConfiguratorTemplateString;
    public GenerateApiConfigurationService() {}

    public Result GenerateApiConfiguratorSourceCode(GeneratorExecutionContext context, string projectNamespace, StubbleVisitorRenderer stubbleBuilder)
    {
        _apiConfiguratorTemplateString ??= Templates.ReadTemplate(Templates.ApiConfiguratorTemplate);
        var source = stubbleBuilder.Render(_apiConfiguratorTemplateString, new BasicClass(projectNamespace));
        context.AddSource(ProjectFileNames.ApiConfigurator, SourceText.From(source, Encoding.UTF8));
        
        return Result.Success();
    }
}