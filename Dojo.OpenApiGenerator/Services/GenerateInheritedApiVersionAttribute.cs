using System.Text;
using CSharpFunctionalExtensions;
using Dojo.OpenApiGenerator.CodeTemplates;
using Dojo.OpenApiGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Stubble.Core;

public class GenerateInheritedApiVersionAttribute : IGenerateInheritedApiVersionAttribute
{
    private string _inheritedApiVersionAttributeTemplateString;

    public GenerateInheritedApiVersionAttribute(){}
    public Result GenerateInheritedApiVersionAttributeSourceCode(GeneratorExecutionContext context, string projectNamespace, StubbleVisitorRenderer stubbleBuilder)
    {
        _inheritedApiVersionAttributeTemplateString ??= Templates.ReadTemplate(Templates.InheritedApiVersionAttribute);

        var source = stubbleBuilder.Render(_inheritedApiVersionAttributeTemplateString, new BasicClass(projectNamespace));

        context.AddSource(ProjectFileNames.InheritedApiVersionAttribute, SourceText.From(source, Encoding.UTF8));

        return Result.Success();
    }
}