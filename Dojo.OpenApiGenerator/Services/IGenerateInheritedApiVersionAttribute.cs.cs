using CSharpFunctionalExtensions;
using Microsoft.CodeAnalysis;
using Stubble.Core;

public interface IGenerateInheritedApiVersionAttribute
{
    Result GenerateInheritedApiVersionAttributeSourceCode(GeneratorExecutionContext context, string projectNamespace, StubbleVisitorRenderer stubbleBuilder);
} 