using System.Threading;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class UpsellGroupController : UpsellGroupControllerBase
    {
        protected override Task<ActionResult<UpsellGroupDraftApiModel>> CreateUpsellGroupAsync(string customerId, UpsertUpsellGroupRequestApiModel upsertUpsellGroupRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchUpsellGroupResponseApiModel>> SearchUpsellGroupAsync(string customerId, SearchUpsellGroupRequestApiModel searchUpsellGroupRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<UpsellGroupDraftApiModel>> GetUpsellGroupAsync(string customerId, string groupId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeleteUpsellGroupAsync(string customerId, string groupId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<UpsellGroupDraftApiModel>> UpdateUpsellGroupAsync(string customerId, string groupId,
            UpsertUpsellGroupRequestApiModel upsertUpsellGroupRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
