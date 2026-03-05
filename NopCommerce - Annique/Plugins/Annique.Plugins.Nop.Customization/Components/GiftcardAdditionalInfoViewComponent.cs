using Annique.Plugins.Nop.Customization.Models.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays additional fields on giftcard info page admin side
    /// </summary>
    [ViewComponent(Name = "GiftcardAdditionalInfo")]
    public class GiftcardAdditionalInfoViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IGiftCardAdditionalInfoService _giftCardAdditionalInfoService;

        #endregion

        #region Ctor

        public GiftcardAdditionalInfoViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IGiftCardAdditionalInfoService giftCardAdditionalInfoService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _giftCardAdditionalInfoService = giftCardAdditionalInfoService;
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

            //get the view model
            if (!(additionalData is GiftCardModel giftCardModel))
                return Content(string.Empty);

            var model = new GiftCardAdditionalInfoModel();

            if (giftCardModel.Id > 0)
            {
                model.GiftCardId = giftCardModel.Id;

                var isExist = _giftCardAdditionalInfoService.GetGiftCardAdditionalInfoByGiftcardId(giftCardModel.Id);
                if (isExist != null)
                { 
                    model.Username = isExist.Username;
                }
            }
            return View(model);
        }

        #endregion
    }
}
