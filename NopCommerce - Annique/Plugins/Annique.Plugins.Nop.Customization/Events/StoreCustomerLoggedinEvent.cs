using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Customer Loggedin event
    /// </summary>
    public class StoreCustomerLoggedinEvent : IConsumer<CustomerLoggedinEvent>
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IGiftService _giftService;
        private readonly IAdditionalActivityLogService  _additionalActivityLogService;

        #endregion

        #region Ctor

        public StoreCustomerLoggedinEvent(IStoreContext storeContext,
            ISettingService settingService,
            IExclusiveItemsService exclusiveItemsService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            ICustomerService customerService,
            IGiftService giftService,
            IHttpContextAccessor httpContextAccessor,
            IAdditionalActivityLogService additionalActivityLogService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _exclusiveItemsService = exclusiveItemsService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _customerService = customerService;
            _giftService = giftService;
            _additionalActivityLogService = additionalActivityLogService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Represents an event that occurs after customer logged in
        /// </summary>
        /// <typeparam name="eventMessage">eventMessage</typeparam>
        public async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //If plugin is enable
            if (settings.IsEnablePlugin)
            {
                //Get current customer
                var customer = eventMessage.Customer;

                //Customer current cart
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                #region Exclusive Items

                //Get exclusive items from table where Force to add cart is true
                var forceAddToCartExclusiveProducts = _exclusiveItemsService.SearchForceAddToCartExclusiveItems(customer.Id);

                foreach (var exclusiveProduct in forceAddToCartExclusiveProducts)
                {
                    //Get product by product id
                    var product = await _productService.GetProductByIdAsync((int)exclusiveProduct.ProductID);

                    //first, try to find product in existing shopping cart 
                    var itemInCart = cart.Where(ci => ci.ProductId == product.Id).FirstOrDefault();

                    //If item alredy not exist in cart
                    if (itemInCart == null)
                    {
                        //Quantity purchased
                        var qtyPurchased = (exclusiveProduct.nQtyPurchased == null) ? 0 : exclusiveProduct.nQtyPurchased;

                        //Quantity available after qauntity purchased
                        var quantity = exclusiveProduct.nQtyLimit - qtyPurchased;

                        //Add exclusive item to cart with available limit quantity
                        await _shoppingCartService.AddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, store.Id,null,0,null,null,(int)quantity,true);
                    }
                }

                #endregion

                #region Force Gift Items

                var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

                //check customer role contains user profile 
                if (customerRoleIds.Contains(settings.ConsultantRoleId))
                {
                    var forceGifts = await _giftService.GetAllForceGiftsAsync();
                    if (forceGifts.Any())
                    {
                        foreach (var gift in forceGifts)
                        {
                            var takenGiftsQtyTotal = await _giftService.GetGiftTakenQtyTotalAsync(gift.Id, customer.Id);
                            if (takenGiftsQtyTotal == 0 || takenGiftsQtyTotal < gift.nQtyLimit)
                                await _giftService.AddGiftProductInShoppingCartAsync(cart, gift.ProductId, 1, customer, store.Id);
                        }
                    }
                }

                #endregion

                #region Task 644 new Activity logs

                await _additionalActivityLogService.InsertActivityTrackingLogAsync("PublicStore.UserLogin", "Login", customer);

                #endregion
            }
        }

        #endregion
    }
}
