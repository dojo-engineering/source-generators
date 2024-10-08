using System.Reflection;
using Xunit;
using Dojo.AutoGenerators;

namespace Dojo.Generators.Tests
{
    public class AutoInterfaceTests
    {
        [Theory]
        [InlineData(
            "StringBuilder DoNonPrimitiveType(StringBuilder bdr)",
            "System.Text.StringBuilder DoNonPrimitiveType(System.Text.StringBuilder bdr)")]
        [InlineData(
            "void Do()",
            "void Do()")]
        [InlineData(
            "int DoDefault(int i = 0, string a = null)",
            "int DoDefault(int i = 0, string a = default(string))")]
        [InlineData(
            "int DoReturn(string i, int? b)",
            "int DoReturn(string i, int? b)")]
        [InlineData(
            "T2 DoGeneric<T1, T2>(T1 t1, T2 t2)",
            "T2 DoGeneric<T1, T2>(T1 t1, T2 t2)")]
        [InlineData(
            "int DefaultKeyword(System.Threading.CancellationToken b = default)",
            "int DefaultKeyword(System.Threading.CancellationToken b = default(System.Threading.CancellationToken))")]
        public void MethodSignature_Generate(string source, string expected)
        {
            // Arrange
            string userSource = $@"
using System;
using System.Text;
namespace Level1.Level2
{{
    [AutoInterface]
    public partial class TestFoo
    {{
        public {source}
        {{
            return null;
        }}
    }}
";

            string expectedSource = $@"
using System;
using System.CodeDom.Compiler;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class TestFoo: ITestFoo
    {{
    }}

    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public interface ITestFoo
    {{
        {expected};
    }}
}}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoInterfaceGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }


        [Fact]
        public void MethodOverloadSignature_Generate()
        {
            // Arrange
            string userSource = $@"
using System;
using System.Text;
namespace Level1.Level2
{{
    [AutoInterface]
    public partial class TestFoo
    {{
        public void Do()
        {{
            return null;
        }}

        public void Do(int a, bool b)
        {{
            return null;
        }}
    }}
";

            string expectedSource = $@"
using System;
using System.CodeDom.Compiler;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class TestFoo: ITestFoo
    {{
    }}

    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public interface ITestFoo
    {{
        void Do();
        void Do(int a, bool b);
    }}
}}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoInterfaceGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }
        
        [Fact]
        public void PropertySignature_Generate()
        {
            // Arrange
            string userSource = $@"
using System;
using System.Text;
namespace Level1.Level2
{{
    [AutoInterface]
    public partial class TestFoo
    {{
        private int _prop2;

        public string Prop1 {{ get; set; }}

        public int Prop2 {{ get {{ return _prop2; }} }}
    }}
";

            string expectedSource = $@"
using System;
using System.CodeDom.Compiler;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class TestFoo: ITestFoo
    {{
    }}

    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public interface ITestFoo
    {{
        string Prop1 {{ get; set; }}
        int Prop2 {{ get; }}
    }}
}}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoInterfaceGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }

        [Theory]
        [InlineData("TestFoo<T>", "TestFoo<T>", "ITestFoo<T>")]
        [InlineData("TestFoo<T>\n            where T : class", "TestFoo<T>", "ITestFoo<T>\n           where T : class")]
        [InlineData("TestFoo<T>\n            where T : class, new()", "TestFoo<T>", "ITestFoo<T>\n           where T : class, new()")]
        [InlineData("TestFoo<T>\n            where T : struct", "TestFoo<T>", "ITestFoo<T>\n           where T : struct")]
        [InlineData("TestFoo<T>\n            where T : Dojo.Generators.Tests.AutoInterfaceTests", "TestFoo<T>", "ITestFoo<T>\n           where T : Dojo.Generators.Tests.AutoInterfaceTests")]
        [InlineData("TestFoo<T1, T2>", "TestFoo<T1, T2>", "ITestFoo<T1, T2>")]
        [InlineData("TestFoo<T1, T2> where T1 : class", "TestFoo<T1, T2>", "ITestFoo<T1, T2>\n           where T1 : class")]
        [InlineData("TestFoo<T1, T2> where T1 : class where T2 : class", "TestFoo<T1, T2>", "ITestFoo<T1, T2>\n           where T1 : class\n           where T2 : class")]
        public void GenericInterface_Generate(string source, string expectedType, string expectedInterface)
        {
            // Arrange
            string userSource = $@"
using System;
using System.Text;
namespace Level1.Level2
{{
    [AutoInterface]
    public partial class {source}
    {{
        public void SomeMethod()
        {{
            return null;
        }}
    }}
}}";

            string expectedSource = $@"
using System;
using System.CodeDom.Compiler;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class {expectedType}: {expectedInterface}
    {{
    }}

    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public interface {expectedInterface}
    {{
        void SomeMethod();
    }}
}}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoInterfaceGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }
    }
}
