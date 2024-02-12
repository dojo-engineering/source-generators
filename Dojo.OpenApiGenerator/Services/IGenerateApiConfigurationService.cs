using CSharpFunctionalExtensions;
using Microsoft.CodeAnalysis;
using Stubble.Core;

public interface IGenerateApiConfigurationService
{
    Result GenerateApiConfiguratorSourceCode(GeneratorExecutionContext context, string projectNamespace, StubbleVisitorRenderer stubbleBuilder);
}