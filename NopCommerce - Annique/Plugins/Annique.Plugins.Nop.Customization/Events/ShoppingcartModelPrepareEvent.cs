using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Annique.Plugins.Nop.Customization.Services.StaffCustomerCheckout;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Events
{
    /// <summary>
    /// Shopping cart model prepare event
    /// </summary>
    public class ShoppingcartModelPrepareEvent : IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IStaffCustomerCheckoutRuleService _staffCustomerCheckoutRuleService;
        private readonly ILocalizationService _localizationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IProductService _productService;
        private readonly IGiftService _giftService;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
      
        #endregion

        #region Ctor

        public ShoppingcartModelPrepareEvent(IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IStaffCustomerCheckoutRuleService staffCustomerCheckoutRuleService,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IPriceFormatter priceFormatter,
            IExclusiveItemsService exclusiveItemsService,
            IRepository<ShoppingCartItem> sciRepository,
            IGiftService giftService,
            IProductService productService,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
            _staffCustomerCheckoutRuleService = staffCustomerCheckoutRuleService;
            _localizationService = localizationService;
            _shoppingCartService = shoppingCartService;
            _priceFormatter = priceFormatter;
            _exclusiveItemsService = exclusiveItemsService;
            _sciRepository = sciRepository;
            _giftService = giftService;
            _productService = productService;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
        }

        #endregion

        #region Utility

        void UpdateCartItemQuantity(ShoppingCartModel.ShoppingCartItemModel cartItem, ShoppingCartItem shoppingCartItem, int quantity)
        {
            // Update cart item model quantity
            cartItem.Quantity = quantity;

            // Update cart item quantity
            shoppingCartItem.Quantity = quantity;
            shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
        }

        async Task UpdateDatabaseAsync(ShoppingCartItem shoppingCartItem, Customer customer)
        {
            // Update cart item in database
            await _sciRepository.UpdateAsync(shoppingCartItem);
            await _customerService.UpdateCustomerAsync(customer);
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
                if (eventMessage.Model is ShoppingCartModel)
                {
                    var model = eventMessage.Model as ShoppingCartModel;

                    //Get current customer
                    var customer = await _workContext.GetCurrentCustomerAsync();

                    //get customer roles id
                    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
                    
                    #region Annique Staff customer order limit validation

                    //check customer role contains annique staff
                    if (customerRoleIds.Contains(settings.CustomerRoleId))
                    {
                        //process staff shopping cart validation
                        model = await _staffCustomerCheckoutRuleService.ProcessStaffShoppingCartValidationsAsync(customer, store, model, settings);
                    }

                    #endregion

                    #region Consultant Profile Validation

                    // Store UserProfileValidation message (if needed)
                    string userProfileValidationMessage = null;

                    userProfileValidationMessage = await _userProfileAdditionalInfoService.ValidateUserProfileAsync(customer,customerRoleIds,model,settings);

                    #endregion

                    model.CustomProperties.Clear();

                    // Add UserProfileValidation if message not null
                    if (userProfileValidationMessage != null)
                    {
                        model.CustomProperties["UserProfileValidation"] = userProfileValidationMessage;
                    }
                    
                    //Check each item
                    foreach (var cartItem in model.Items)
                    {
                        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);

                        #region Rsp price 

                        // Add custom property to show Total RSP
                        var totalRSPKey = "TotalRsp_" + cartItem.Id;

                        var rspPrice = decimal.Zero;

                        // This ensures that the total price (rspPrice) only includes products that have a positive unit price.
                        if (cartItem.UnitPriceValue > decimal.Zero)
                            rspPrice = product.Price;

                        model.CustomProperties[totalRSPKey] = await _priceFormatter.FormatPriceAsync(rspPrice, true, false);

                        #endregion

                        //Get cart item 
                        var shoppingCartItem = await _sciRepository.GetByIdAsync(cartItem.Id, cache => default);

                        #region Exclusive Item Quantity Limit

                        //Get exclusive item by product id and customer id
                        var exclusiveItem = await _exclusiveItemsService.GetAllocatedExclusiveItemAsync(cartItem.ProductId, customer.Id);
                        if (exclusiveItem != null)
                        {
                            //Quantity purchased
                            var qtyPurchased = (exclusiveItem.nQtyPurchased == null) ? 0 : exclusiveItem.nQtyPurchased;

                            //Quantity available after qauntity purchased
                            var quantity = exclusiveItem.nQtyLimit - qtyPurchased;

                            if (quantity == 0)
                            {
                                model.Items.Remove(cartItem);

                                //Update cart item in database
                                await _sciRepository.DeleteAsync(shoppingCartItem);
                                await _customerService.UpdateCustomerAsync(customer);
                                return;
                            }

                            if (quantity != 0 && cartItem.Quantity > quantity)
                            {
                                //Add warning
                                cartItem.Warnings = new List<string>()
                                {
                                    string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumQuantity"), quantity)
                                };

                                UpdateCartItemQuantity(cartItem, shoppingCartItem, (int)quantity);
                                await UpdateDatabaseAsync(shoppingCartItem, customer);

                                cartItem.SubTotalValue = (decimal)(cartItem.UnitPriceValue * quantity);
                                cartItem.SubTotal = await _priceFormatter.FormatPriceAsync(cartItem.SubTotalValue, true, false);
                            }
                        }

                        #endregion

                        //check customer role contains annique consultant
                        if (customerRoleIds.Contains(settings.ConsultantRoleId))
                        {
                            #region Checkout Gift Products

                            var gift = await _giftService.GetGiftByProductIdAsync(cartItem.ProductId);
                            if (gift != null)
                            {
                                var takenGiftsQtyTotal = await _giftService.GetGiftTakenQtyTotalAsync(gift.Id, customer.Id);

                                // available qty
                                var availableQuantity = gift.nQtyLimit - takenGiftsQtyTotal;
                                if (cartItem.Quantity > availableQuantity)
                                {
                                    UpdateCartItemQuantity(cartItem, shoppingCartItem, availableQuantity);
                                    await UpdateDatabaseAsync(shoppingCartItem, customer);
                                }

                                // if product has any warning
                                if (cartItem.Warnings.Any())
                                {
                                    // Update cart
                                    await _shoppingCartService.UpdateShoppingCartItemAsync(customer, cartItem.Id, string.Empty, 0, null, null, 0, false);
                                    return; // continue to avoid null exception for next block
                                }

                                if (takenGiftsQtyTotal == 0 || takenGiftsQtyTotal < gift.nQtyLimit)
                                {
                                    if (gift.cGiftType == AnniqueCustomizationDefaults.GiftTypeForce || gift.cGiftType == AnniqueCustomizationDefaults.GiftTypeStarter)
                                        cartItem.DisableRemoval = true;
                                }

                            }

                            #endregion
                        }

                    }
                }
            }
        }

        #endregion
    }
}
