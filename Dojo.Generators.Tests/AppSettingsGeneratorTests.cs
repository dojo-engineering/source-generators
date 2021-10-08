using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dojo.AutoGenerators;
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
        public void SimpleStringValue_Generate()
        {
            // Arrange
            const string actualSettins = @"
{
    SettingString: ""value""
}
";
            const string expectedSource = @"";
            var content = new AdditionalTextContent("appsettings.json", actualSettins);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(""appsettings.json"")]
    partial class MySettings{}
}
";

            var expected = @$"
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class MySettings
    {{
        public string SettingString {{get;}}


        public AppSettings(IConfiguration configuration)
        {{
            if (configuration == null)
            {{
                throw new ArgumentNullException(nameof(configuration));
            }}


            this.SettingString = configuration[""SettingString""];

        }}
    }}
}}";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new(){content}, out var _, new AppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();
    
            // Assert
            GeneratorTestHelper.CompareSources(expected, syntaxTries[1].ToString());
        }
        
        [Fact]
        public void SimpleObjectValue_Generate()
        {
            // Arrange
            const string actualSettins = @"
{
    SettingObject: {""name"": ""value""}
}
";
            const string expectedSource = @"";
            var content = new AdditionalTextContent("appsettings.json", actualSettins);
            const string source = @"
namespace Level1.Level2 {
    [AutoAppSettings(""appsettings.json"")]
    partial class MySettings{}
}
";

            var expected1 = @$"
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class MySettings
    {{
        public SettingObjectSettings SettingObject {{get;}}


        public AppSettings(IConfiguration configuration)
        {{
            if (configuration == null)
            {{
                throw new ArgumentNullException(nameof(configuration));
            }}


            this.SettingObject = new SettingObjectSettings();
            configuration.GetSection(""SettingObject"").Bind(this.SettingObject);

        }}
    }}
}}";
            var expected2 = $@"
using System;
using System.CodeDom.Compiler;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class SettingObjectSettings
    {{
        public string name {{ get; set; }}

    }}
}}";
            // Act
            Compilation comp = GeneratorTestHelper.CreateCompilation(source);
            var newComp = GeneratorTestHelper.RunGenerators(comp, new(){content}, out var _, new AppSettingsGenerator());
            var syntaxTries = newComp.SyntaxTrees.ToList();
    
            // Assert
            GeneratorTestHelper.CompareSources(expected1, syntaxTries[1].ToString());
            GeneratorTestHelper.CompareSources(expected2, syntaxTries[2].ToString());
        }
    }
}