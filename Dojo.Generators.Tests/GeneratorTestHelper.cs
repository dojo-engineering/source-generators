using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Dojo.Generators.Tests
{
    public static class GeneratorTestHelper
    {
        public static Compilation CreateCompilation(string source) => CreateCompilation(new[] { source });

        public static Compilation CreateCompilation(params string[] sources) => CSharpCompilation.Create(
             assemblyName: "compilation",
             syntaxTrees: sources.Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Preview))),
             references: new[]
             {
                 MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                 MetadataReference.CreateFromFile(typeof(Dojo.Generators.Abstractions.AutoInterfaceAttribute).GetTypeInfo().Assembly.Location)
             },
             options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
         );

        private static GeneratorDriver CreateDriver(Compilation compilation, ImmutableArray<AdditionalText> additionalTexts,  params ISourceGenerator[] generators) => CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create(generators),
            additionalTexts: additionalTexts,
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            optionsProvider: null
        );

        public static Compilation RunGenerators(Compilation compilation, List<AdditionalText> additionalFiles, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            var additionalTexts = ImmutableArray<AdditionalText>.Empty;
            if(additionalFiles != null)
            {
                additionalTexts = ImmutableArray.Create(additionalFiles.ToArray());
            }

            var driver = CreateDriver(compilation, additionalTexts, generators);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);
            return updatedCompilation;
        }

        public static string GenerateFromSource<T>(string source, List<AdditionalText> additionalFiles = null) where T: ISourceGenerator, new()
        {
            Compilation comp = CreateCompilation(source);
            var newComp = RunGenerators(comp, additionalFiles, out var _, new T());

            Assert.True(newComp.SyntaxTrees.Count() >= 2);
            return newComp.SyntaxTrees.ToList()[1].ToString();
        }

        public static string GenerateFromSourceAt<T>(string source, int treeIndex, List<AdditionalText> additionalFiles = null) where T: ISourceGenerator, new()
        {
            Compilation comp = CreateCompilation(source);
            var newComp = RunGenerators(comp, additionalFiles, out var _, new T());

            var trees = newComp.SyntaxTrees.ToList();
            Assert.True(trees.Count > treeIndex, $"Expected at least {treeIndex + 1} syntax trees but got {trees.Count}.");
            return trees[treeIndex].ToString();
        }

        public static void CompareSources(string expected, string actual)
        {
            var expectedLines = string.Join("\r\n", expected.Replace("\r", "")
                    .Split("\n")
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            var actualLines = string.Join("\r\n", actual.Replace("\r", "")
                                .Split("\n")
                                .Where(s => !string.IsNullOrWhiteSpace(s)));

            Assert.Equal(expectedLines, actualLines);
        }
    }
}
