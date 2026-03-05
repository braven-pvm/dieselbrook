using Annique.Plugins.Nop.Customization.Models.PickUpCollection;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays in Postnet store delivery pop up
    /// </summary>
    [ViewComponent(Name = "PostNetStoreDelivery")]
    public class PostNetStoreDeliveryViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        #endregion

        #region Ctor

        public PostNetStoreDeliveryViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
           _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            var isPickUpCollection = await _anniqueCustomizationConfigurationService.IsPickupCollectionEnableAsync();

            if (!isPickUpCollection)
                return Content(string.Empty);

            var model = new PostNetStoreDeliveryModel();

            return View(model);
        }

        #endregion
    }
}

