using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class MenuApiController : Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206.MenuManagementApiControllerBase
    {
        protected override Task<ActionResult<SearchMenuResponseApiModel>> SearchMenuAsync(string customerId, SearchMenuRequestApiModel searchMenuRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

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

        protected override Task<ActionResult<MenuApiModel>> GetPublishedMenuAsync(string locationId, string accountId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeletePublishedMenuAsync(string locationId, string accountId, string menuId, CancellationToken cancellationToken)
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

        protected override Task<ActionResult<MenuDraftApiModel>> CreateDraftMenuAsync(string customerId, CreateMenuDraftRequestApiModel createMenuDraftRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchDraftMenuResponseApiModel>> SearchDraftMenuAsync(string customerId, SearchDraftMenuRequestApiModel searchDraftMenuRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuDraftApiModel>> GetDraftMenuAsync(string customerId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> SoftDeleteDraftMenuAsync(string customerId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuDraftApiModel>> UpdateDraftMenuAsync(string customerId, string menuId, UpdateMenuDraftRequestApiModel updateMenuDraftRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuCategoryDraftApiModel>> AddDraftMenuCategoriesAsync(string customerId, string menuId,
            CreateMenuCategoryRequestApiModel createMenuCategoryRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchCategoriesResponseApiModel>> SearchDraftMenuCategoriesAsync(string customerId, string menuId,
            SearchCategoriesRequestApiModel searchCategoriesRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuCategoryDraftApiModel>> GetDraftMenuCategoryDetailsAsync(string customerId, string menuId, string categoryId,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeleteDraftMenuCategoryDetailsAsync(string customerId, string menuId, string categoryId,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuCategoryDraftApiModel>> UpdateDraftMenuCategoryDetailsAsync(string customerId, string menuId, string categoryId,
            UpdateMenuCategoryRequestApiModel updateMenuCategoryRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuApiModel>> PublishDraftMenuAsync(string customerId, string menuId, PublishMenusRequestApiModel publishMenusRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
