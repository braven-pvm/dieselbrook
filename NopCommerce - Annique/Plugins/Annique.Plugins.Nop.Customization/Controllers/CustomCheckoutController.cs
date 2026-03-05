using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using Annique.Plugins.Nop.Customization.Factories.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Models.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Models.QuickCheckout;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using Annique.Plugins.Nop.Customization.Services.ShippingAddressValidation;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class CustomCheckoutController : BasePublicController
    {
        #region Fields

        private readonly IShippingAddressValidationService _shippingAddressValidationService;
        private readonly ILocalizationService _localizationService;
        private readonly IGiftModelFactory _giftModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IGiftService _giftService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private const int MinimumAvailableQuantity = 0;
        private readonly ISpecialOffersService _specialOffersService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly CustomerSettings _customerSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public CustomCheckoutController(IShippingAddressValidationService shippingAddressValidationService,
            ILocalizationService localizationService,
            IGiftModelFactory giftModelFactory,
            IStoreContext storeContext,
            IWorkContext workContext,
            IGiftService giftService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            ILogger logger,
            IExclusiveItemsService exclusiveItemsService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            ISpecialOffersService specialOffersService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IAuthenticationService authenticationService,
            IEventPublisher eventPublisher,
            CustomerSettings customerSettings,
            IWorkflowMessageService workflowMessageService,
            ICustomerActivityService customerActivityService)
        {
            _shippingAddressValidationService = shippingAddressValidationService;
            _localizationService = localizationService;
            _giftModelFactory = giftModelFactory;
            _storeContext = storeContext;
            _workContext = workContext;
            _giftService = giftService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _logger = logger;
            _exclusiveItemsService = exclusiveItemsService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _specialOffersService = specialOffersService;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _authenticationService = authenticationService;
            _eventPublisher = eventPublisher;
            _customerSettings = customerSettings;
            _workflowMessageService = workflowMessageService;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Utilities

        //update gift item in gift model property
        private async Task UpdateGiftItemAsync(GiftModel.GiftItemsModel giftItem, ShoppingCartItem itemInUpdatedCart)
        {
            if (itemInUpdatedCart != null)
            {
                if (itemInUpdatedCart.Quantity > giftItem.AvailableQuanitity)
                {
                    itemInUpdatedCart.Quantity = giftItem.AvailableQuanitity;

                    //Update cart
                    await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                                itemInUpdatedCart.Id, string.Empty, decimal.Zero,
                                null, null, itemInUpdatedCart.Quantity, true);
                }

                giftItem.AvailableQuanitity = Math.Max(MinimumAvailableQuantity, giftItem.AvailableQuanitity - itemInUpdatedCart.Quantity);
                if (giftItem.AvailableQuanitity == 0)
                    giftItem.IsAlreadyInCart = true;

                // Calculation for available qty dropdown
                giftItem.AvailableQuantities.Clear();
                if (giftItem.AvailableQuanitity > 0)
                {
                    for (var i = 0; i <= giftItem.AvailableQuanitity; i++)
                    {
                        giftItem.AvailableQuantities.Insert(i, new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                    }
                }
            }
        }

        //update exclusive item gift model property
        private async Task UpdateExclusiveItemAsync(GiftModel.ExclusiveItemsModel exclusiveItem, ShoppingCartItem itemInUpdatedCart)
        {
            if (itemInUpdatedCart != null)
            {
                if (itemInUpdatedCart.Quantity > exclusiveItem.AvailableQuanitity)
                {
                    itemInUpdatedCart.Quantity = exclusiveItem.AvailableQuanitity;

                    //Update cart
                    await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                                itemInUpdatedCart.Id, string.Empty, decimal.Zero,
                                null, null, itemInUpdatedCart.Quantity, true);
                }

                exclusiveItem.AvailableQuanitity = Math.Max(MinimumAvailableQuantity, exclusiveItem.AvailableQuanitity - itemInUpdatedCart.Quantity);
                if (exclusiveItem.AvailableQuanitity == 0)
                    exclusiveItem.IsAlreadyInCart = true;

                // Calculation for available qty dropdown
                exclusiveItem.AvailableQuantities.Clear();
                if (exclusiveItem.AvailableQuanitity > 0)
                {
                    for (var i = 0; i <= exclusiveItem.AvailableQuanitity; i++)
                    {
                        exclusiveItem.AvailableQuantities.Insert(i, new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                    }
                }
            }
        }

        #endregion

        #region Method

        [CheckLanguageSeoCode(ignore: true)]
        public virtual async Task<IActionResult> ProvinceSearchTermAutoComplete(int stateId, string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new
                {
                    success = false,
                    message = await _localizationService.GetResourceAsync("Annique.Plugin.ShippingAddressValidation.Field.Term"),
                });
            }
            term = term.Trim();

            if (stateId == 0)
            {
                return Json(new
                {
                    success = false,
                    message = await _localizationService.GetResourceAsync("Annique.Plugin.ShippingAddressValidation.Field.State"),
                });
            }

            var subrubCombinations = await _shippingAddressValidationService.GetSubrubCombinationsAsync(term, stateId);

            //If no Combinations available
            if (!subrubCombinations.Any())
                return Json(new
                {
                    success = false,
                    message = await _localizationService.GetResourceAsync("Annique.Plugin.ShippingAddressValidation.Term.NotFound")
                });

            var result = (from p in subrubCombinations
                          select new
                          {
                              label = p.Value
                          })
                .ToList();
            return Json(result);
        }

		public async Task<IActionResult> GiftProductPopUp()
		{
			try
			{
				var customer = await _workContext.GetCurrentCustomerAsync();
				var store = await _storeContext.GetCurrentStoreAsync();
				var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

				if (!cart.Any())
				{
					return Json(new { Result = false, redirect = Url.RouteUrl("Checkout") });
				}

				var isConsultantUser = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();
				if (!isConsultantUser)
				{
					return await HandleNonConsultantUserAsync(cart);
				}

				return await HandleConsultantUserAsync(cart, customer);
			}
			catch (Exception exc)
			{
				await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
				return Json(new { error = 1, message = exc.Message });
			}
		}

		private async Task<JsonResult> HandleNonConsultantUserAsync(IList<ShoppingCartItem> cart)
		{
            var specialOffers = await _specialOffersService.GetActiveSpecialOfferListAsync();
            if (specialOffers.Any())
            {
				var model = new GiftModel
				{
					SpecialOffers = await _giftModelFactory.PrepareSpecialOfferModelAsync(specialOffers, cart)
				};

				if (model.SpecialOffers.Any())
				{
					var giftPopUpHtml = await RenderPartialViewToStringAsync("_GiftProductPopUp", model);
					var itemCount = model.SpecialOffers.Count;

					return Json(new { Result = true, TotalItems = itemCount, htmldata = giftPopUpHtml });
				}
				return Json(new { Result = false, redirect = Url.RouteUrl("Checkout") });
			} 
            return Json(new { Result = false, redirect = Url.RouteUrl("Checkout") });		
        }

		private async Task<JsonResult> HandleConsultantUserAsync(IList<ShoppingCartItem> cart, Customer customer)
		{
			var canProcessGifts = await _giftService.CanProcessGiftsAsync(cart, customer);

			if (canProcessGifts)
			{
				var productIdsInCart = cart.Select(item => item.ProductId).ToArray();
				var gifts = await _giftService.GetGiftsByProductIdsAsync(productIdsInCart);
				var cartWithoutGifts = _giftService.FilterItemsWithoutGifts(cart, gifts);

				var (subTotalWithoutDiscount, _) = await _anniqueCustomizationConfigurationService.GetShoppingCartTotalsBeforeDiscountAsync(cartWithoutGifts);
				var blankGifts = await _giftService.GetAllBlankGiftsAsync(subTotalWithoutDiscount);
				var exclusiveItems = await _exclusiveItemsService.SearchStarterKitExclusiveItemsAsync(customer.Id);
                var specialOffers = await _specialOffersService.GetActiveSpecialOfferListAsync();

                if (blankGifts.Any() || exclusiveItems.Any() || specialOffers.Any()) 
                { 
                    return await RenderGiftPopupAsync(blankGifts, exclusiveItems, specialOffers ,cart); 
                }

				#region if no blank gift available based on order total then get and remove old blank gift product from cart

				// Filter out items with blank gifts
				var blankGiftCartItemsList = cart.Where(item =>
				{
					var gift = gifts.FirstOrDefault(g => g.ProductId == item.ProductId);
					return gift != null && string.IsNullOrWhiteSpace(gift.cGiftType);
				}).ToList();


				foreach (var blankGiftItem in blankGiftCartItemsList)
				{
					//remove Product from cart 
					await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
								   blankGiftItem.Id, string.Empty, decimal.Zero,
								   null, null, 0, true);
				}

				#endregion
			}

			return Json(new { Result = false, redirect = Url.RouteUrl("Checkout") });
		}

		private async Task<JsonResult> RenderGiftPopupAsync(IList<Gift> blankGifts, IList<ExclusiveItems> exclusiveItems, IList<(Offers, Discount)> activeOffers , IList<ShoppingCartItem> cart)
		{
			var model = await _giftModelFactory.PrepareBlankGiftModelAsync(blankGifts, exclusiveItems, activeOffers ,cart);

			if (model.GiftItems.Any() || model.ExclusiveItems.Any() || model.SpecialOffers.Any())
			{
				var giftPopUpHtml = await RenderPartialViewToStringAsync("_GiftProductPopUp", model);
				var itemCount = model.GiftItems.Count + model.ExclusiveItems.Count + model.SpecialOffers.Count;

				return Json(new { Result = true, TotalItems = itemCount, htmldata = giftPopUpHtml });
			}

			return Json(new { Result = false, redirect = Url.RouteUrl("Checkout") });
		}

		[HttpPost]
        //Post method for Gift product pop up which perform add to cart functionality for selected gift products
        public async Task<IActionResult> GiftProductPopUp(GiftModel model, int productId, int quantity, bool isDonation)
        {
            try
            {
                //if qty is not 0 then add gift product to shopping cart
                if (quantity != 0)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();

                    var store = await _storeContext.GetCurrentStoreAsync();

                    //Customer current cart
                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                    //Get product
                    var product = await _productService.GetProductByIdAsync(productId);

                    //first, try to find product in existing shopping cart 
                    var itemInCart = cart.Where(item => item.ProductId == product.Id).FirstOrDefault();

                    //If product not in cart add to cart
                    if (itemInCart == null)
                    {
                        //Add item to cart
                        await _shoppingCartService.AddToCartAsync(customer, product, ShoppingCartType.ShoppingCart, store.Id, null, 0, null, null, quantity, true);
                    }
                    else
                    {
                        if (!isDonation)
                            quantity += itemInCart.Quantity;

                        //Update cart
                        await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                                    itemInCart.Id, string.Empty, decimal.Zero,
                                    null, null, quantity, true);
                    }

                    //Check for Exclusive starter Item
                    var exclusiveStarterProduct = _exclusiveItemsService.IsStarterExclusiveProduct(product.Id, customer.Id);

                    //if starter exclusive product added to cart then find starter gift from cart and remove 
                    if (exclusiveStarterProduct)
                    {
                        //Get gift id and shopping cart item Id for already exist starter gifttype product in cart
                        var starterGiftItem = await _giftService.GetExistGiftItemInCartAsync(customer.Id, AnniqueCustomizationDefaults.GiftTypeStarter, cart);

                        if (starterGiftItem.Count > 0 && starterGiftItem[0].sciId != 0)
                        {
                            //Remove old starer gift product from cart
                            await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                                starterGiftItem[0].sciId, string.Empty, decimal.Zero,
                                null, null, 0, true);
                        }
                    }
                    //Customer current cart
                    var updatedCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                    var itemInUpdatedCart = updatedCart.Where(i => i.ProductId == productId).FirstOrDefault();

                    // Check if the product is a gift
                    var isGiftItem = model.GiftItems.Any(item => item.ProductId == productId);

                    // Check if the product is an exclusive item
                    var isExclusiveItem = model.ExclusiveItems.Any(item => item.ProductId == productId);

                    if (isDonation)
                    {
                        // Donation item handling
                        if (itemInUpdatedCart != null)
                        {
                            model.DonationProductQtyInCart = itemInUpdatedCart.Quantity;
                        }
                    }
                    else if (isGiftItem)
                    {
                        // Get Gift item from model
                        var giftItem = model.GiftItems.FirstOrDefault(item => item.ProductId == productId);

                        //update giftitem
                        await UpdateGiftItemAsync(giftItem, itemInUpdatedCart);
                    }
                    else if (isExclusiveItem)
                    {
                        // Exclusive item from model
                        var exclusiveItem = model.ExclusiveItems.FirstOrDefault(item => item.ProductId == productId);

                        //update exclusive item
                        await UpdateExclusiveItemAsync(exclusiveItem, itemInUpdatedCart);
                    }

                    var giftPopUpHtml = await RenderPartialViewToStringAsync("_GiftProductPopUp", model);

                    return Json(new
                    {
                        success = true,
                        htmlData = giftPopUpHtml
                    });
                }
                return Json(new
                {
                    success = false
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecialOfferProducts(int offerId, int discountId)
        {
            try
            {
                var model = await _giftModelFactory.PrepareSpecialProductListModelAsync(offerId, discountId);

                var productPopUpHtml = await RenderPartialViewToStringAsync("_SpecialProductPopUp", model);

                return Json(new { success = true, htmldata = productPopUpHtml });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { success = false, error = 1, message = "An error occurred while processing your request.", details = exc.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SpecialOfferProductAddToCart(List<ProductSelectionModel> products, int selectedOfferId)
        {
            if (products == null || !products.Any())
            {
                return Json(new { success = false, message = "No products selected." });
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var productIds = products.Select(p => p.Id).ToArray();
            var specialProducts = await _productService.GetProductsByIdsAsync(productIds);
            var specialOffer = await _specialOffersService.GetOfferByIdAsync(selectedOfferId);

            //Customer current cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var cartType = ShoppingCartType.ShoppingCart;
            var addedProductIds = new List<int>();

            try
            {
                #region Allowed selection validation

                var productIdsF = await _specialOffersService.GetProductIdsByOfferTypeAsync(specialOffer.Id, "F");

                // Calculate the current allowed selections
                var allowedSelections = await _specialOffersService.CalculateAllowedSelectionsAsync(specialOffer, productIdsF, cart);

                allowedSelections = _specialOffersService.AdjustAllowedSelectionsBasedOnCartGProducts(specialOffer, allowedSelections, cart);

                // Calculate the total quantity of selected products
                var totalSelectedQuantity = products.Sum(p => p.Quantity);

                // Validate if the total quantity exceeds allowed selections
                if (totalSelectedQuantity > allowedSelections)
                {
                    return Json(new { success = false, message = $"The total quantity of selected products exceeds the allowed selections. Maximum allowed: {allowedSelections}" });
                }

                #endregion

                #region Special offer Add to cart

                foreach (var product in products)
                {
                    var selectedProduct = specialProducts.FirstOrDefault(p => p.Id == product.Id);

                    if (selectedProduct == null)
                    {
                        continue; // Skip if the product is not found
                    }

                    // Custom attributes
                    var attributeXml = await _specialOffersService.AddSpecialOfferAttributeAsync(string.Empty, selectedOfferId, specialOffer.DiscountId);
                    // Check if the product with the same custom attributes is already in the cart
                    var existingCartItem = cart.FirstOrDefault(c => c.ProductId == product.Id && c.AttributesXml == attributeXml);

                    // If we already have the same product in the cart, then use the total quantity to validate
                    var quantityToValidate = existingCartItem != null ? existingCartItem.Quantity + product.Quantity : product.Quantity;

                    var addToCartWarnings = await _shoppingCartService
                        .GetShoppingCartItemWarningsAsync(customer, cartType, selectedProduct, store.Id, string.Empty, decimal.Zero, null, null, quantityToValidate, false, existingCartItem?.Id ?? 0, true, false, false, false);

                    if (addToCartWarnings.Any())
                    {
                        // Modify the warnings to include the product name
                        var modifiedWarnings = addToCartWarnings.Select(warning => $"{selectedProduct.Name}: {warning}").ToArray();

                        // Cannot be added to the cart, display standard warnings
                        return Json(new
                        {
                            success = false,
                            message = modifiedWarnings,
                            addedProductIds = addedProductIds
                        });
                    }

                    if (existingCartItem != null)
                    {
                        await _shoppingCartService.UpdateShoppingCartItemAsync(customer, existingCartItem.Id, existingCartItem.AttributesXml, existingCartItem.CustomerEnteredPrice, existingCartItem.RentalStartDateUtc, existingCartItem.RentalEndDateUtc, quantityToValidate, true);
                    }
                    else
                    {
                        // Add new product to cart
                        await _shoppingCartService.AddToCartAsync(customer, selectedProduct, cartType, store.Id, attributeXml, 0, null, null, product.Quantity, true);
                    }

                    addedProductIds.Add(product.Id);
                }

                return Json(new { success = true, message = "Products added to cart successfully!", addedProductIds = addedProductIds });

                #endregion
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding products to cart.", details = ex.Message });
            }
        }

        #endregion

        #region Login / registration methods

        [HttpPost("checkout-login")]
        [ValidateAntiForgeryToken]
        [CheckAccessClosedStore(ignore: true)]
        [CheckAccessPublicStore(ignore: true)]
        public async Task<IActionResult> Login([FromBody] CheckoutLoginModal model)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage).FirstOrDefault()
                                                    ?? "Invalid input";
                return Json(new { success = false, message = errorMessage });
            }

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(model.Username?.Trim(), model.Password?.Trim());

            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = await _customerService.GetCustomerByUsernameAsync(model.Username);
                        await _customerRegistrationService.SignInCustomerAsync(customer, string.Empty);
                        return Json(new { success = true });
                    }
                case CustomerLoginResults.WrongPassword:
                case CustomerLoginResults.Deleted:
                case CustomerLoginResults.NotActive:
                case CustomerLoginResults.NotRegistered:
                case CustomerLoginResults.CustomerNotExist:
                case CustomerLoginResults.LockedOut:
                default:
                    var message = await _localizationService.GetResourceAsync("Account.Login.WrongCredentials");
                    return Json(new { success = false, message });
            }
        }

        [HttpPost("checkout-register")]
        [ValidateAntiForgeryToken]
        [CheckAccessClosedStore(ignore: true)]
        [CheckAccessPublicStore(ignore: true)]
        public async Task<IActionResult> Register([FromBody] CheckoutRegisterModel model)
        {
            // Server-side model validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var store = await _storeContext.GetCurrentStoreAsync();
            customer.RegisteredInStoreId = store.Id;

            var customerUserName = model.Email?.Trim();
            var customerEmail = model.Email?.Trim();
            customer.Phone = model.Phone?.Trim();

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;

            var registrationRequest = new CustomerRegistrationRequest(
                customer,
                customerEmail,
                _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                model.Password,
                _customerSettings.DefaultPasswordFormat,
                store.Id,
                isApproved
            );

            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);

            if (registrationResult.Success)
            {
                customer.FirstName = model.Name?.Trim();
                customer.Phone = model.Phone?.Trim();

                await _customerService.UpdateCustomerAsync(customer);

                await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                await _anniqueCustomizationConfigurationService.SetClientRoleToUserAsync(customer);

                var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                switch (_customerSettings.UserRegistrationType)
                {
                    case UserRegistrationType.Standard:
                        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);
                        await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));
                        await SignInCustomerAsync(customer, true);
                        return Json(new { success = true });

                    default:
                        return Json(new { success = false, message = "Registration type not supported via this method." });
                }
            }

            // If there are errors
            var errorMessage = string.Join(" ", registrationResult.Errors);
            return Json(new { success = false, message = errorMessage });
        }

        public virtual async Task SignInCustomerAsync(Customer customer, bool isPersist = false)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (currentCustomer?.Id != customer.Id)
            {
                //migrate shopping cart
                await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

                await _workContext.SetCurrentCustomerAsync(customer);
            }

            //sign in new customer
            await _authenticationService.SignInAsync(customer, isPersist);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);
        }
        #endregion
    }
}
