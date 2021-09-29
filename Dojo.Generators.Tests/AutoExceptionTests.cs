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

            const string expectedSource = @"
using System;
using System.Runtime.Serialization;

namespace Level1.Level2
{
    public partial class TestException : Exception
    {
        public TestException()
        {
        }

        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}";
            // Act
            var actual = GeneratorTestHelper.GenerateFromSource<AutoExceptionGenerator>(userSource);

            // Assert
            GeneratorTestHelper.CompareSources(expectedSource, actual);
        }
    }
}
