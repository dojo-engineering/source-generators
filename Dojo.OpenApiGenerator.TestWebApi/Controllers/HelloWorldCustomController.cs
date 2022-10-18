using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V1;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models;
using Dojo.OpenApiGenerator.TestWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class HelloWorldCustomController : HelloWorldControllerBase
    {
        private readonly IHelloWorldService _helloWorldService;

        public HelloWorldCustomController(IHelloWorldService helloWorldService)
        {
            _helloWorldService = helloWorldService;
        }

        public override Task<ActionResult<HelloFromSourceApiModel>>
            HelloFromSourceActionAsync([FromQuery, BindRequired, MaxLength(5)] System.String message, [FromRoute, BindRequired] System.Int64 number)
        {
            return base.HelloFromSourceActionAsync(message, number);
        }

        protected override async Task<ActionResult<HelloFromSourceApiModel>> HelloFromSourceAsync(string message, long number)
        {
            return Ok(await _helloWorldService.HelloFromSourceAsync(number));
        }

        protected override async Task<ActionResult<string>> GetHelloGenerated2Async()
        {
            return Ok(await _helloWorldService.HelloGenerated2Async());
        }
    }
}
