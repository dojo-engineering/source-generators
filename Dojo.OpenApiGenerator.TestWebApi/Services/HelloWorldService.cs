using System;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.Exceptions;
using Dojo.OpenApiGenerator.TestWebApi.Models;

namespace Dojo.OpenApiGenerator.TestWebApi.Services
{
    public class HelloWorldService : IHelloWorldService
    {
        public Task<HelloFromSourceApiModel> HelloFromSourceAsync(long number)
        {
            var result = GetHelloFromSourceGeneratedApiModel(number);

            if (result.Number % 2 == 0)
            {
                throw new NotFoundApiException("Hello Source not found!");
            }

            return Task.FromResult(result);
        }

        public Task<string> HelloGenerated2Async()
        {
            return Task.FromResult("Hello Generated 3");
        }

        private static HelloFromSourceApiModel GetHelloFromSourceGeneratedApiModel(long number)
        {
            var now = DateTime.UtcNow;
            return new HelloFromSourceApiModel
            {
                DateTime = now,
                Message = $"Hello from {now}",
                Number = number
            };
        }
    }
}
