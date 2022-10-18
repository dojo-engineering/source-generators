using System;
using Microsoft.AspNetCore.Http;

namespace Dojo.OpenApiGenerator.TestWebApi.Exceptions
{
    public class NotFoundApiException : ApiException
    {

        public NotFoundApiException() : base(StatusCodes.Status404NotFound)
        {
        }

        public NotFoundApiException(string message) : base(StatusCodes.Status404NotFound, message)
        {
        }

        public NotFoundApiException(string message, Exception inner) : base(StatusCodes.Status404NotFound, message, inner)
        {
        }
    }
}
