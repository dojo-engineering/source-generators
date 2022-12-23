using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20220407;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20220407;
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
            HelloFromSourceAsync([FromQuery, BindRequired, MaxLength(5)] System.String message, [FromRoute, BindRequired] System.Int64 number)
        {
            return base.HelloFromSourceAsync(message, number);
        }

        protected override async Task<ActionResult<HelloFromSourceApiModel>> HelloFromSourceExecutorAsync(string message, long number)
        {
            return Ok(await _helloWorldService.HelloFromSourceAsync(number));
        }

        protected override async Task<ActionResult<string>> GetHelloGenerated2ExecutorAsync()
        {
            return Ok(await _helloWorldService.HelloGenerated2Async());
        }

        protected override Task<ActionResult<ExtendedHelloFromSourceApiModel>> ExtendedGetHelloGeneratedExecutorAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<string>> ObsoleteGetHelloGenerated2ExecutorAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
