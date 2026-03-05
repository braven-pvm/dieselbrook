using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Shopping cart model prepare event
    /// </summary>
    public class OrderTotalsModelPrepareEvent : IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IGiftService _giftService;

        #endregion

        #region Ctor

        public OrderTotalsModelPrepareEvent(IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IShoppingCartService shoppingCartService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IGiftService giftService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _shoppingCartService = shoppingCartService;
            _giftService = giftService;
            _anniqueCustomizationConfigurationService= anniqueCustomizationConfigurationService;
        }

        #endregion
        #region Method

        /// <summary>
        /// Represents an event that occurs after Shoppingcart model prepare
        /// </summary>
        /// <typeparam name="eventMessage">eventMessage</typeparam>
        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //If plugin is enable
            if (settings.IsEnablePlugin)
            {
                if (eventMessage.Model is OrderTotalsModel)
                {
                    var model = eventMessage.Model as OrderTotalsModel;

                    //current customer cart
                    var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
                    
                    //cart total before discount
                    var (cartTotal, cartTotalValue) = await _anniqueCustomizationConfigurationService.GetShoppingCartTotalsBeforeDiscountAsync(shoppingCartItems);
                   
                    //discount amount
                    var discountValue = await _giftService.GetShoppingCartTotalsDiscountAsync(shoppingCartItems, cartTotal);

                    model.CustomProperties.Clear();
                    model.CustomProperties["TotalRSP"] = cartTotal.ToString();
                    model.CustomProperties["TotalRSPAmount"] = cartTotalValue;
                    model.CustomProperties["TotalDiscountAmount"] = discountValue;
                }
            }
        }

        #endregion
    }
}
