using System.Threading;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class ModifierGroupController : ModifierGroupControllerBase
    {
        protected override Task<ActionResult<ModifierGroupDraftApiModel>> CreateModifierGroupAsync(string customerId, UpsertModifierGroupRequestApiModel upsertModifierGroupRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchModifierGroupResponseApiModel>> SearchModifierGroupAsync(string customerId, SearchModifierGroupRequestApiModel searchModifierGroupRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<ModifierGroupDraftApiModel>> GetModifierGroupAsync(string customerId, string groupId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeleteModifierGroupAsync(string customerId, string groupId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<ModifierGroupDraftApiModel>> UpdateModifierGroupAsync(string customerId, string groupId,
            UpsertModifierGroupRequestApiModel upsertModifierGroupRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
