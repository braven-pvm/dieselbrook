using Annique.Plugins.Nop.Customization.Factories;
using Annique.Plugins.Nop.Customization.Models;
using Annique.Plugins.Nop.Customization.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AdminCategoryIntegrationController : BaseAdminController
    {
        #region Fields

        private readonly ICategoryIntegrationModelFactory _categoryIntegrationModelFactory;
        private readonly ICategoryIntegrationService _categoryIntegrationService;
        private readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public AdminCategoryIntegrationController(ICategoryIntegrationModelFactory categoryIntegrationModelFactory,
            ICategoryIntegrationService categoryIntegrationService,
            ICategoryService categoryService)
        {
            _categoryIntegrationModelFactory= categoryIntegrationModelFactory;
            _categoryIntegrationService= categoryIntegrationService;
            _categoryService= categoryService;
        }

        #endregion

        #region Categoey Integration List / Add / Delete

        [HttpPost]
        public virtual async Task<IActionResult> CategoryIntegrationList(CategoryIntegrationSearchModel searchModel)
        {
            var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId);
            if (category == null)
                return NotFound();

            var categoryIntegrationList = await _categoryIntegrationModelFactory.PrepareCategoryIntegrationListModelAsync(searchModel, category.Id);

            return Json(categoryIntegrationList);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CategoryIntegrationAdd(CategoryIntegrationModel model)
        {
            if (ModelState.IsValid)
            {
                var categoryIntegration = _categoryIntegrationModelFactory.PrepareCategoryIntegrationFields(model);

                await _categoryIntegrationService.InsertCategoryIntegrationAsync(categoryIntegration);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CategoryIntegrationDelete(int id)
        {
            var categoryIntegration = await _categoryIntegrationService.GetCategoryIntegrationAsync(id);
            if (categoryIntegration == null)
                return NotFound();

            await _categoryIntegrationService.DeleteCategoryIntegrationAsync(categoryIntegration);

            return new NullJsonResult();
        }

        #endregion
    }
}
