using Xunit;

namespace DojoGenerator.Tests
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
            "int DoDefault(int i = 0, string a = null)")]
        [InlineData(
            "int DoReturn(string i, int? b)",
            "int DoReturn(string i, int? b)")]
        [InlineData(
            "T2 DoGeneric<T1, T2>(T1 t1, T2 t2)",
            "T2 DoGeneric<T1, T2>(T1 t1, T2 t2)")]
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
namespace Level1.Level2
{{
    public partial class TestFoo: ITestFoo
    {{
    }}

    public interface ITestFoo
    {{
        {expected};
    }}
}}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoExceptionGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }
    }
}
