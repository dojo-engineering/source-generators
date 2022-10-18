using System;
using Microsoft.AspNetCore.Http;

namespace Dojo.OpenApiGenerator.TestWebApi.Exceptions
{
    public class BadRequestApiException : ApiException
    {

        public BadRequestApiException() : base(StatusCodes.Status400BadRequest)
        {
        }

        public BadRequestApiException(string message) : base(StatusCodes.Status400BadRequest, message)
        {
        }

        public BadRequestApiException(string message, Exception inner) : base(StatusCodes.Status400BadRequest, message, inner)
        {
        }
    }
}
