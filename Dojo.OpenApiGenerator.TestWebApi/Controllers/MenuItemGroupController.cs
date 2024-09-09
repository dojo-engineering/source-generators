using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class MenuItemGroupController : MenuItemGroupControllerBase
    {
        protected override Task<ActionResult<IEnumerable<MenuItemGroupApiModel>>> GetMenuItemGroupAsync(string customerId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuItemGroupApiModel>> CreateMenuItemGroupAsync(string customerId, UpsertMenuItemGroupRequestApiModel upsertMenuItemGroupRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeleteMenuItemGroupAsync(string customerId, string menuItemGroupId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuItemGroupApiModel>> UpdateMenuItemGroupAsync(string customerId, string menuItemGroupId,
            UpsertMenuItemGroupRequestApiModel upsertMenuItemGroupRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
