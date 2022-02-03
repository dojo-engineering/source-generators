using System;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20220103;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class ConnectECredentialsController : ConnectECredentialsControllerBase
    {
        protected override Task<ExternalAuthResponseApiModel> GetConnectECredentialsAsync(string accountId, string locationId, string version)
        {
            throw new NotImplementedException();
        }
    }
}
