using Annique.Plugins.Nop.Customization.Models.Catalog;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class CustomShoppingCartController : BasePublicController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILogger _logger;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        #endregion

        #region Ctor

        public CustomShoppingCartController(IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IWorkContext workContext,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ILogger logger,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _workContext = workContext;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _logger = logger;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
        }

        #endregion

        #region Method

        //returns catalog product details
        public async Task<IActionResult> GetCatalogProductDetails(int[] productIds)
        {
            try
            {
                //get active store
                var store = await _storeContext.GetCurrentStoreAsync();

                //Get current customer
                var customer = await _workContext.GetCurrentCustomerAsync();

                var catalogProductDetails = new List<CatalogProductDetails>();

                if (productIds == null || productIds.Length == 0)
                {
                    return Json(catalogProductDetails);
                }

                productIds = productIds.Distinct().ToArray();

                //Customer current cart
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (cart.Any())
                {
                    //first, try to find products in existing shopping cart 
                    var itemInCart = cart.Where(item => productIds.Contains(item.ProductId)).ToList();
                    foreach (var item in itemInCart)
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        var price = await _priceFormatter.FormatPriceAsync(product.Price, false, false);
                        var qty = item.Quantity;
                        catalogProductDetails.Add(new CatalogProductDetails { ProductId = item.ProductId, Quantity = qty, Price = price });
                    }

                    //other products which does not exist in cart
                    var filterProductIds = productIds.Except(cart.Select(item => item.ProductId)).ToArray();

                    if (filterProductIds.Length > 0)
                    {
                        //get all products
                        var productDetails = await Task.WhenAll(filterProductIds.Select(_productService.GetProductByIdAsync));

                        //prepare product details with product price and cart qty 0
                        catalogProductDetails.AddRange(productDetails.Select(async product =>
                        {
                            decimal price = product.Price;
                            string formattedPrice = await _priceFormatter.FormatPriceAsync(price, false, false);

                            return new CatalogProductDetails
                            {
                                ProductId = product.Id,
                                Quantity = 0,
                                Price = formattedPrice,
                            };
                        }).Select(task => task.Result));
                    }
                }
                else
                {
                    var productDetails = await Task.WhenAll(productIds.Select(_productService.GetProductByIdAsync));
                    catalogProductDetails.AddRange(productDetails.Select(async product =>
                    {
                        decimal price = product.Price;
                        string formattedPrice = await _priceFormatter.FormatPriceAsync(price, false, false);

                        return new CatalogProductDetails
                        {
                            ProductId = product.Id,
                            Quantity = 0,
                            Price = formattedPrice,
                        };
                    }).Select(task => task.Result));
                }

                return Json(catalogProductDetails);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public async Task<IActionResult> GetProductCartItemQuantity(int productId)
        {
            try
            {
                //get active store
                var store = await _storeContext.GetCurrentStoreAsync();

                //Get current customer
                var customer = await _workContext.GetCurrentCustomerAsync();

                //Customer current cart
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                int cartItemQuantity = 0;

                if (cart.Any())
                {
                    //first, try to find product in existing shopping cart 
                    var itemInCart = cart.Where(item => productId == item.ProductId).FirstOrDefault();

                    if (itemInCart != null)
                        cartItemQuantity = itemInCart.Quantity;
                }

                return Json(cartItemQuantity);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public async Task<IActionResult> GetCartTotalValue()
        {
            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            //Get current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            //Customer current cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var (_, cartTotalValue) = await _anniqueCustomizationConfigurationService.GetShoppingCartTotalsBeforeDiscountAsync(cart);
            return Json(new { cartTotalValue });
        }

        #endregion
    }
}
