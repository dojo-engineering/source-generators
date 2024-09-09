using System.Threading;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class MenuItemController : MenuItemControllerBase
    {
        protected override Task<ActionResult<MenuItemDraftApiModel>> CreateMenuItemAsync(string customerId, UpsertMenuItemRequestApiModel upsertMenuItemRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchMenuItemResponseApiModel>> SearchMenuItemAsync(string customerId, SearchMenuItemRequestApiModel searchMenuItemRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuItemDraftApiModel>> GetMenuItemDetailsAsync(string customerId, string menuItemId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeleteMenuItemAsync(string customerId, string menuItemId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuItemDraftApiModel>> UpdateMenuItemAsync(string customerId, string menuItemId, UpsertMenuItemRequestApiModel upsertMenuItemRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
