using System.Threading.Tasks;
using Dojo.Generators.Abstractions;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers;
using Dojo.OpenApiGenerator.TestWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    [AutoControllerOverride("HelloWorld")]
    public class HelloWorldCustomController : HelloWorldController
    {
        public HelloWorldCustomController(IHelloWorldService helloWorldService) : base(helloWorldService)
        {
        }

        public override Task<IActionResult> HelloFromSourceAsync(long number)
        {
            return Task.FromResult<IActionResult>(Ok(number));
        }
    }
}
