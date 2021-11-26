using Dojo.Generators.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    [AutoController]
    [Route("[controller]")]
    public class HelloWorldController : GenerateHelloWorldController
    {
        [Route("hello-2")]
        [HttpGet]
        public override IActionResult Hello() => base.Hello();
    }
}
