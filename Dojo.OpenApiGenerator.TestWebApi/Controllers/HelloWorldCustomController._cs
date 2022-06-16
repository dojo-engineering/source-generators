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

        protected override async Task<ActionResult<HelloFromSourceApiModel>> HelloFromSourceAsync(long number, string message)
        {
            return Ok(await _helloWorldService.HelloFromSourceAsync(number));
        }

        protected override async Task<ActionResult<string>> HelloGenerated2Async()
        {
            return Ok(await _helloWorldService.HelloGenerated2Async());
        }
    }
}
