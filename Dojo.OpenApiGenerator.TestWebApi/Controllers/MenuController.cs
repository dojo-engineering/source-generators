using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class MenuController : MenuControllerBase
    {
        protected override Task<ActionResult<MenuDraftApiModel>> CreateDraftMenuAsync(string customerId, CreateMenuDraftRequestApiModel createMenuDraftRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> DeletePublishedMenuAsync(string locationId, string accountId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuDraftApiModel>> GetDraftMenuAsync(string customerId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuApiModel>> GetPublishedMenuAsync(string locationId, string accountId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuApiModel>> PublishDraftMenuAsync(string customerId, string menuId, PublishMenusRequestApiModel publishMenusRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchDraftMenuResponseApiModel>> SearchDraftMenuAsync(string customerId, SearchDraftMenuRequestApiModel searchDraftMenuRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<SearchMenuResponseApiModel>> SearchMenuAsync(string customerId, SearchMenuRequestApiModel searchMenuRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IActionResult> SoftDeleteDraftMenuAsync(string customerId, string menuId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ActionResult<MenuDraftApiModel>> UpdateDraftMenuAsync(string customerId, string menuId, UpdateMenuDraftRequestApiModel updateMenuDraftRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
