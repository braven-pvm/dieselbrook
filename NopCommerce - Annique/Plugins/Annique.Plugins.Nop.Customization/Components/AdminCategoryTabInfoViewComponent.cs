using Annique.Plugins.Nop.Customization.Factories;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays in admin category integration
    /// </summary>
    [ViewComponent(Name = "AdminCategoryTabInfo")]
    public class AdminCategoryTabInfoViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly ICategoryIntegrationModelFactory _categoryIntegrationModelFactory;

        #endregion

        #region Ctor

        public AdminCategoryTabInfoViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            ICategoryIntegrationModelFactory categoryIntegrationModelFactory)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _categoryIntegrationModelFactory = categoryIntegrationModelFactory;
        }

        #endregion

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return Content(string.Empty);

            if (!widgetZone?.Equals(AdminWidgetZones.CategoryDetailsBlock, StringComparison.InvariantCultureIgnoreCase) ?? true)
                return Content(string.Empty);

            //get the view model
            if (!(additionalData is CategoryModel categoryModel))
                return Content(string.Empty);

            //prepare model
            var model = _categoryIntegrationModelFactory.PrepareCustomCategoryTabModelInfoAsync(categoryModel.Id);
            return View(model);
        }

        #endregion
    }
}