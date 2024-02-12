using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dojo.OpenApiGenerator.Configuration;
using Microsoft.CodeAnalysis;
using Stubble.Core;

public interface IGenerateApisSourceCode
{
    Result GenerateApiSourceCode(GeneratorExecutionContext context, string projectNamespace, StubbleVisitorRenderer stubbleBuilder, ICollection<string> apisToOverride, string projectDir, AutoApiGeneratorSettings autoApiGeneratorSettings);
}