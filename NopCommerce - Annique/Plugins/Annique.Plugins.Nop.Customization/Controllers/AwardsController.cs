using Annique.Plugins.Nop.Customization.Factories.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Models.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Components;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AwardsController : BasePublicController
    {
        #region Field

        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IAwardsModelFactory _awardsModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IAwardService _awardService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly ShoppingCartSettings  _shoppingCartSettings;

        #endregion

        #region Ctor

        public AwardsController(ICustomerService customerService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IAwardsModelFactory awardsModelFactory,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IAwardService awardService,
            IRepository<ShoppingCartItem> sciRepository,
            ILogger logger,
            ILocalizationService localizationService,
            ShoppingCartSettings shoppingCartSettings)
        {
            _customerService = customerService;
            _workContext = workContext;
            _storeContext = storeContext;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _awardsModelFactory = awardsModelFactory;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _awardService = awardService;
            _sciRepository = sciRepository;
            _logger = logger;
            _localizationService = localizationService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region method

        //Awards List
        public virtual async Task<IActionResult> AwardList()
        {
            //Check for consultant role
            var consultantRole = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();

            if (!consultantRole)
                return InvokeHttp404();

            var customer = await _workContext.GetCurrentCustomerAsync();

            var model = await _awardsModelFactory.PrepareAwardListModelAsync(customer.Id);

            if (!model.Awards.Any())
                return InvokeHttp404();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AwardProductAddToCart(List<AwardProductQuantityModel> products, int selectedAwardId)
        {
            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _workContext.GetCurrentCustomerAsync();

                //Customer current cart
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                // Access the selected award ID and the list of products with quantities
                var award = await _awardService.GetAwardByIdAsync(selectedAwardId);
                if (products != null)
                {
                    // Perform the add to cart functionality for each product in the list
                    foreach (var product in products)
                    {
                        //Get product by product id
                        var awardProduct = await _productService.GetProductByIdAsync(product.ProductId);

                        var attributeXml = _awardService.AddAwardProductAttribute(string.Empty, award.Id, award.AwardType);
                        // Check if the product already exists in the cart 
                        var existingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, ShoppingCartType.ShoppingCart, awardProduct, attributeXml);

                        if (existingCartItem != null)
                        {
                            //get temp award shopping cart item data
                            var exisingAwardShoppingCartItem = await _awardService.GetAwardScibyShoppingCartItemIdAsync(existingCartItem.Id);

                            //if temp cart qty and new qty same then continue because nothing to update
                            if (exisingAwardShoppingCartItem != null && exisingAwardShoppingCartItem.Quantity == product.Quantity)
                                continue;

                            await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                                                existingCartItem.Id, attributeXml, decimal.Zero,
                                                null, null, product.Quantity, true);

                            //update quantity in temp award shopping cart item table
                            exisingAwardShoppingCartItem.Quantity = product.Quantity;
                            await _awardService.UpdateAwardShoppingCartItemsAsync(exisingAwardShoppingCartItem);
                        }
                        else
                        {
                            //If the product is not in the cart, add it to the cart
                            await _shoppingCartService.AddToCartAsync(customer, awardProduct, ShoppingCartType.ShoppingCart, store.Id, attributeXml, 0, null, null, product.Quantity, true);

                            var newlyAddedItem = await _sciRepository.Table
                                                .OrderByDescending(sci => sci.CreatedOnUtc)
                                                .FirstOrDefaultAsync(sci => sci.CustomerId == customer.Id && sci.StoreId == store.Id && sci.ProductId == product.ProductId);

                            if (newlyAddedItem != null)
                                await _awardService.InsertAwardShoppingCartItemAsync(selectedAwardId, newlyAddedItem);
                        }
                    }
                }

                //display notification message and update appropriate blocks
                var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                var updateTopCartSectionHtml = string.Format(
                    await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                    shoppingCarts.Sum(item => item.Quantity));

                var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                    ? await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent))
                    : string.Empty;

                // Return a response if necessary
                return Json(new { success = true,
                    message = await _localizationService.GetResourceAsync("Award.ProductAddToCartMessage"),
                    updatetopcartsectionhtml = updateTopCartSectionHtml,
                    updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public async Task<IActionResult> GetAwardProductQuantities(int awardId)
        {
            // Fetch the updated product quantities for the specified awardId
            var quantities = await _awardsModelFactory.PrepareAwardProductQuantityModelAsync(awardId);

            // Return the quantities as JSON using the JsonResult method
            return Json(quantities);
        }

        #endregion
    }
}
