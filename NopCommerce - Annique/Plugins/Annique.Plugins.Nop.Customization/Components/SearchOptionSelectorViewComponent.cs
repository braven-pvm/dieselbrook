using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays search radio button selection
    /// </summary>
    [ViewComponent(Name = "SearchOptionSelector")]
    public class SearchOptionSelectorViewComponent :  NopViewComponent
    {
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        public SearchOptionSelectorViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
        }

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isEnabled = await _anniqueCustomizationConfigurationService.IsFullTextSearchEnableAsync();
            if (!isEnabled)
                return Content(string.Empty); // render nothing if disabled

            return View();
        }
    }
}
