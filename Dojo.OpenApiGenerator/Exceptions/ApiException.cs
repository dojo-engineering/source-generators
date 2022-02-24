using System;

namespace Dojo.OpenApiGenerator.Exceptions
{
    public class ApiException : Exception
    {
        public int HttpStatusCode { get; }

        public ApiException(int httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
        }

        public ApiException(int httpStatusCode, string message)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        public ApiException(int httpStatusCode, string message, Exception inner)
            : base(message, inner)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
