using System.Threading;
using System.Threading.Tasks;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Controllers.V20240206;
using Dojo.OpenApiGenerator.TestWebApi.Generated.Models.V20240206;
using Microsoft.AspNetCore.Mvc;

namespace Dojo.OpenApiGenerator.TestWebApi.Controllers
{
    public class MenuCategoryController : MenuCategoryControllerBase
    {
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
    }
}
