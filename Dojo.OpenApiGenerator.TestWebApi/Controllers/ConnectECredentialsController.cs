using System;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V10;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    [ApiVersion("1.0")]
    public class ConnectECredentialsController : ConnectECredentialsControllerBase
    {
        protected override Task<ExternalAuthReponseApiModel> GetConnectECredentialsAsync(string locationId, string remoteId, string version)
        {
            throw new NotImplementedException();
        }

        protected override Task<ExternalAuthReponseApiModel> ObsoleteGetConnectECredentialsAsync(string locationId, string remoteId, string version)
        {
            throw new NotImplementedException();
        }
    }
}
