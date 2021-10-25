using System.Linq;
using System.Threading;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Dojo.AutoGenerators.AppSettingsGenerator;
using Dojo.AutoGenerators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Dojo.Generators.Tests
{
    internal class AdditionalTextContent : AdditionalText
    {
        public AdditionalTextContent(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Content { get; }

        public override string Path {get;}

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(Content);
        }
    }

    [UseReporter(typeof(DiffReporter))]
    public class AppSettingsGeneratorTests
    {
        [Fact]
        
        public void RunGenerator_SimpleStringRootLevelValue_ExpectCorrectFilesProduced()
        {
            // Arrange
            const string actualSettings = @"
{
    SettingString: ""value""
}
";
            var content = new AdditionalTextContent("appsettings.json", actualSettings);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(""appsettings.json"")]
    partial class MySettings{}
}
";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new(){content}, out var _, new AutoAppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();

            // Assert
            Approvals.Verify(syntaxTries[1].ToString());
        }

        [Fact]

        public void RunGenerator_SimpleStringRootLevelValueWithoutSpecifyingAppSettingsFileExplicitly_ExpectCorrectFilesProduced()
        {
            // Arrange
            const string actualSettings = @"
{
    SettingString: ""value""
}
";
            var content = new AdditionalTextContent("appsettings.json", actualSettings);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings]
    partial class MySettings{}
}
";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new() { content }, out var _, new AutoAppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();

            // Assert
            Approvals.Verify(syntaxTries[1].ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void RunGenerator_SimpleNestedObject_ExpectCorrectNestedCSharpFileGenerated(int generatedUnitNumber)
        {
            // Arrange
            NamerFactory.AdditionalInformation = generatedUnitNumber.ToString();

            const string actualSettings = @"
{
    SettingObject: {""name"": ""value""}
}
";
            var content = new AdditionalTextContent("appsettings.json", actualSettings);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(""appsettings.json"")]
    partial class MySettings{}
}
";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new() { content }, out var _, new AutoAppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();

            // Assert
            Approvals.Verify(syntaxTries[2].ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        public void RunGenerator_ComplexRealLifeAppConfig_ExpectCorrectAppSettingsFileProduced(int generatedUnitNumber)
        {
            // Arrange
            NamerFactory.AdditionalInformation = generatedUnitNumber.ToString();
            string actualSettings = ReadComplexAppSettings(0);
            var content = new AdditionalTextContent("appsettings.json", actualSettings);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(""appsettings.json"")]
    partial class AppSettings {}
}
";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new() { content }, out var _, new AutoAppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();

            // Assert
            Approvals.Verify(syntaxTries[generatedUnitNumber].ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void RunGenerator_ComplexRealLifeAppConfigWithConventionalTypes_ExpectCorrectAppSettingsFileProduced(int generatedUnitNumber)
        {
            // Arrange
            NamerFactory.AdditionalInformation = generatedUnitNumber.ToString();
            string actualSettings = ReadComplexAppSettings(1);
            var content = new AdditionalTextContent("appsettings.json", actualSettings);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(FileName= ""appsettings.json"", UseConventionalTypes =  ""true"", ExcludedSections = new string[] { ""Logging""  })]
    partial class AppSettings {}
}
";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new() { content }, out var _, new AutoAppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();

            // Assert
            Approvals.Verify(syntaxTries[generatedUnitNumber].ToString());
        }

        private static string ReadComplexAppSettings(int index)
        {
            string resourceName = $"Dojo.Generators.Tests.TestFiles.appsettings.{index}.json";
            return AssemblyUtils.ReadEmbeddedResource(resourceName);
        }
    }
}