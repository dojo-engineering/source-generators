using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models;
using Dojo.OpenApiGenerator.TestWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class HelloWorldCustomController : HelloWorldControllerBase
    {
        private readonly IHelloWorldService _helloWorldService;

        public HelloWorldCustomController(IHelloWorldService helloWorldService)
        {
            _helloWorldService = helloWorldService;
        }

        public override Task<IActionResult> HelloFromSourceActionAsync(long number)
        {
            return Task.FromResult<IActionResult>(Ok(number));
        }

        protected override Task<HelloFromSourceApiModel> HelloFromSourceAsync(long number)
        {
            return _helloWorldService.HelloFromSourceAsync(number);
        }

        protected override Task<string> HelloGenerated2Async()
        {
            return _helloWorldService.HelloGenerated2Async();
        }
    }
}
