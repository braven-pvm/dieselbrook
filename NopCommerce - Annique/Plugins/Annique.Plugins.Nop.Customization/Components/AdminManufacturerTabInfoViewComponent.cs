using System.Threading.Tasks;
using System;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Areas.Admin.Models.Catalog;
using Annique.Plugins.Nop.Customization.Factories;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays in admin manufacturer integration
    /// </summary>
    [ViewComponent(Name = "AdminManufacturerTabInfo")]
    public class AdminManufacturerTabInfoViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IManufacturerIntegrationModelFactory _manufacturerIntegrationModelFactory;
        
        #endregion

        #region Ctor

        public AdminManufacturerTabInfoViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IManufacturerIntegrationModelFactory manufacturerIntegrationModelFactory)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _manufacturerIntegrationModelFactory = manufacturerIntegrationModelFactory;
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

            if (!widgetZone?.Equals(AdminWidgetZones.ManufacturerDetailsBlock, StringComparison.InvariantCultureIgnoreCase) ?? true)
                return Content(string.Empty);

            //get the view model
            if (!(additionalData is ManufacturerModel manufacturerModel))
                return Content(string.Empty);

            //prepare model
            var model = _manufacturerIntegrationModelFactory.PrepareCustomManufacturerTabModelInfoAsync(manufacturerModel.Id);
            return View(model);
        }

        #endregion
    }
}
