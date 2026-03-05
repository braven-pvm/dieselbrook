using Annique.Plugins.Nop.Customization.Factories.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays list of available giftcards list
    /// </summary>
    [ViewComponent(Name = "GiftcardList")]
    public class GiftcardListViewComponent : NopViewComponent
    {
        #region Field

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IGiftCardAdditionalInfoService _giftCardAdditionalInfoService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardAdditionalInfoModelFactory _giftCardAdditionalInfoModelFactory;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public GiftcardListViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IGiftCardAdditionalInfoService giftCardAdditionalInfoService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IGiftCardAdditionalInfoModelFactory giftCardAdditionalInfoModelFactory,
            ShoppingCartSettings shoppingCartSettings)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _giftCardAdditionalInfoService = giftCardAdditionalInfoService;
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
            _giftCardAdditionalInfoModelFactory = giftCardAdditionalInfoModelFactory;
            _shoppingCartSettings = shoppingCartSettings;
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
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();

            var isConsultant = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();
            if (!isConsultant)
                return Content(string.Empty);

            var isShowGiftCardBox = _shoppingCartSettings.ShowGiftCardBox;
            if (!isShowGiftCardBox)
                return Content(string.Empty);

            //Get allocated giftcards
            var availableGiftCards = await _giftCardAdditionalInfoService.GetAllocatedGiftCardsByUsernameAsync(customer.Username);
            if (availableGiftCards.Any())
            {
                //Prepare giftcard info list model
                var model = await _giftCardAdditionalInfoModelFactory.PrepareGiftCardInfoListModelAsync(availableGiftCards);
                return View(model);
            }
            return Content(string.Empty);
        }

        #endregion
    }
}


