using System.Reflection;
using Xunit;
using Dojo.AutoGenerators;

namespace Dojo.Generators.Tests
{
    public class AutoExceptionTests
    {
        [Fact]
        public void SimpleException_Generate()
        {
            // Arrange
            const string userSource = @"
using System;
using System.Text;
namespace Level1.Level2
{
    [AutoException]
    public partial class TestException
    {
    }
}";

            string expectedSource = @$"
using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Level1.Level2
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class TestException : Exception
    {{
        public TestException()
        {{
        }}

        public TestException(string message) : base(message)
        {{
        }}

        public TestException(string message, Exception innerException) : base(message, innerException)
        {{
        }}
    }}
}}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoExceptionGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }
    }
}
