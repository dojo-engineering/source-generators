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
        private static Compilation CreateCompilation(string source) => CSharpCompilation.Create(
             assemblyName: "compilation",
             syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) },
             references: new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
             options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
         );

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) => CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create(generators),
            additionalTexts: ImmutableArray<AdditionalText>.Empty,
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            optionsProvider: null
        );

        private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);
            return updatedCompilation;
        }

        public static string GenerateFromSource<T>(string source) where T: ISourceGenerator, new()
        {
            Compilation comp = CreateCompilation(source);
            var newComp = RunGenerators(comp, out var _, new T());

            Assert.Equal(2, newComp.SyntaxTrees.Count());
            return newComp.SyntaxTrees.ToList()[1].ToString();
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