using System;
using System.Threading.Tasks;
using Dojo.Generators.Abstractions;
using Dojo.OpenApiGenerator.TestWebApi.Exceptions;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models;

namespace Dojo.OpenApiGenerator.TestWebApi.Services
{
    [AutoInterface]
    public partial class HelloWorldService
    {
        public Task<Dojo.OpenApiGenerator.TestWebApi.Generated.Models.HelloFromSourceApiModel> HelloFromSourceAsync(long number)
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
