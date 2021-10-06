using System.IO;
using System.Threading;
using Dojo.AppSettingsGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Dojo.Generators.Tests
{
    internal class AdditionalTextContent : AdditionalText
    {
        public  AdditionalTextContent(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Content { get; }
        public override string Path {get;}

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(this.Content);
        }
    }

    public class AppSettingsGeneratorTests
    {
        [Fact]
        public void SimpleException_Generate()
        {
            // Arrange
            const string expectedSource = @"";
            var content = new AdditionalTextContent("appsettings.json", File.ReadAllText("./TestFiles/appsettings.json"));
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(""appsettings.json"")]
    partial class MySettings{}
}
"; 
            // Act

            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new(){content}, out var _, new AppSettingsGenerator());


            // Assert
           // GeneratorTestHelper.CompareSources(expectedSource, actual);
        }
    }
}