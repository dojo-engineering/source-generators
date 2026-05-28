using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Dojo.OpenApiGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Dojo.Generators.Tests
{
    public class OpenApiGeneratorOutputFolderTests
    {
        [Fact]
        public void RunGenerator_WithoutOutputFolder_ExpectDefaultHintNames()
        {
            var generatedHintNames = RunGeneratorAndGetHintNames(outputFolder: null);

            Assert.Contains("ApiConfigurator.g.cs", generatedHintNames);
            Assert.Contains("ApiConstants.g.cs", generatedHintNames);
            Assert.DoesNotContain(generatedHintNames, hintName => hintName.StartsWith("Generated/OpenApi/", StringComparison.Ordinal));
        }

        [Fact]
        public void RunGenerator_WithOutputFolder_ExpectPrefixedHintNames()
        {
            var generatedHintNames = RunGeneratorAndGetHintNames("Generated/OpenApi");

            Assert.Contains("Generated/OpenApi/ApiConfigurator.g.cs", generatedHintNames);
            Assert.Contains("Generated/OpenApi/ApiConstants.g.cs", generatedHintNames);
            Assert.All(generatedHintNames, hintName => Assert.StartsWith("Generated/OpenApi/", hintName, StringComparison.Ordinal));
        }

        private static List<string> RunGeneratorAndGetHintNames(string outputFolder)
        {
            var projectDirectory = CreateTemporaryProjectDirectory(outputFolder);

            try
            {
                var source = "namespace TestProject { public class Marker { } }";
                var compilation = GeneratorTestHelper.CreateCompilation(source);

                var generator = new AutoApiGenerator();
                GeneratorDriver driver = CSharpGeneratorDriver.Create(
                    generators: ImmutableArray.Create<ISourceGenerator>(generator),
                    parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                    optionsProvider: new TestAnalyzerConfigOptionsProvider(projectDirectory));

                driver = driver.RunGenerators(compilation);
                var runResult = driver.GetRunResult();
                var generatedHintNames = runResult.Results.Single().GeneratedSources.Select(x => x.HintName).ToList();

                Assert.NotEmpty(generatedHintNames);

                return generatedHintNames;
            }
            finally
            {
                Directory.Delete(projectDirectory, recursive: true);
            }
        }

        private static string CreateTemporaryProjectDirectory(string outputFolder)
        {
            var projectDirectory = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
            Directory.CreateDirectory(projectDirectory);
            Directory.CreateDirectory(Path.Combine(projectDirectory, "OpenApiSchemas"));

            const string projectFileContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>TestProject</RootNamespace>
  </PropertyGroup>
</Project>
";
            File.WriteAllText(Path.Combine(projectDirectory, "TestProject.csproj"), projectFileContent);

            const string openApiSchema = @"
{
  ""openapi"": ""3.0.1"",
  ""info"": {
    ""title"": ""DemoApi"",
    ""version"": ""1.0""
  },
  ""paths"": {
    ""/test"": {
      ""get"": {
        ""operationId"": ""GetTest"",
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""ok""
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {},
    ""parameters"": {}
  }
}
";
            File.WriteAllText(Path.Combine(projectDirectory, "OpenApiSchemas", "demo.json"), openApiSchema);

            var settingsContent = outputFolder == null
                ? "{ \"GenerateApiExplorer\": true, \"OrganizeControllersByTags\": false }"
                : $"{{ \"GenerateApiExplorer\": true, \"OrganizeControllersByTags\": false, \"OutputFolder\": \"{outputFolder}\" }}";

            File.WriteAllText(Path.Combine(projectDirectory, "autoApiGeneratorSettings.json"), settingsContent);

            return projectDirectory;
        }

        private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly AnalyzerConfigOptions _globalOptions;

            public TestAnalyzerConfigOptionsProvider(string projectDirectory)
            {
                _globalOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>
                {
                    ["build_property.projectdir"] = projectDirectory
                });
            }

            public override AnalyzerConfigOptions GlobalOptions => _globalOptions;
            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new TestAnalyzerConfigOptions();
            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new TestAnalyzerConfigOptions();
        }

        private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _values;

            public TestAnalyzerConfigOptions(Dictionary<string, string> values = null)
            {
                _values = values ?? new Dictionary<string, string>();
            }

            public override bool TryGetValue(string key, out string value)
            {
                return _values.TryGetValue(key, out value);
            }
        }
    }
}
