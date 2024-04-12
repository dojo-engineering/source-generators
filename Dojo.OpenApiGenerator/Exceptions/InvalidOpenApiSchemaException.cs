using SharpYaml.Schemas;
using System;

namespace Dojo.OpenApiGenerator.Exceptions
{
    public class InvalidOpenApiSchemaException : Exception
    {
        public InvalidOpenApiSchemaException()
        {
        }

        public InvalidOpenApiSchemaException(string message) : base(message)
        {
        }

        public InvalidOpenApiSchemaException(string message, string apiSchemaFile) : base($"{message} from Api File: {apiSchemaFile}")
        {
        }

        public InvalidOpenApiSchemaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidOpenApiSchemaException(string message, Exception innerException, string apiSchemaFile) : base($"{message} from Api File: {apiSchemaFile}", innerException)
        {
        }
    }
}
