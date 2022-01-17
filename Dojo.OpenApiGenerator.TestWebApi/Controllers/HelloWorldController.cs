using Dojo.OpenApiGenerator.TestWebApi.Services;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class HelloWorldController : HelloWorldGeneratedController
    {
        public HelloWorldController(IHelloWorldGeneratedService helloWorldService) : base(helloWorldService)
        {
        }

        //[ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        //public override Task<IActionResult> HelloFromSourceAsync(long number)
        //{
        //    return Task.FromResult<IActionResult>(Ok(number));
        //}
    }
}
