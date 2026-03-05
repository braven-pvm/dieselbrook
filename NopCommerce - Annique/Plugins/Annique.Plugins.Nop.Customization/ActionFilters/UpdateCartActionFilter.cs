using Annique.Plugins.Nop.Customization.Services.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class UpdateCartActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IEventService _eventService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardAdditionalInfoService _giftCardAdditionalInfoService;
        private readonly ISpecialOffersService _specialOffersService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public UpdateCartActionFilter(IStoreContext storeContext,
            ISettingService settingService,
            IEventService eventService,
            IWorkContext workContext,
            ICustomerService customerService,
            IGiftCardAdditionalInfoService giftCardAdditionalInfoService,
            ISpecialOffersService specialOffersService,
            IShoppingCartService shoppingCartService,
            IProductService productService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _eventService = eventService;
            _workContext = workContext;
            _customerService = customerService;
            _giftCardAdditionalInfoService = giftCardAdditionalInfoService;
            _specialOffersService = specialOffersService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
        }

        #endregion

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = (ControllerActionDescriptor)context.ActionDescriptor;

            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var customer = await _workContext.GetCurrentCustomerAsync();

            //get customer roles id
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            if (controller.ControllerTypeInfo == typeof(ShoppingCartController) && context.HttpContext.Request.Method.ToString() == "POST" &&
                       controller.ActionName.Equals("Cart"))
            {
                //If plugin is enable
                if (settings.IsEnablePlugin)
                {
                    //Check form key available or not
                    if (context.HttpContext.Request.Form.Keys.Any(x => x.Equals("removefromcart", StringComparison.InvariantCultureIgnoreCase)) ||
                        context.HttpContext.Request.Form.Keys.Any(x => x.StartsWith("itemquantity", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (context.ActionArguments.TryGetValue("form", out var formValue) && formValue is FormCollection form)
                        {
                            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                            var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
                            .ToDictionary(item => item.Id, item => item);

                            //get identifiers of items to remove
                            var itemIdsToRemove = form["removefromcart"]
                                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                                .Distinct().ToList();

                            var ids = itemIdsToRemove.Select(id => id).ToList();

                            // Handle event-related functionality
                            await _eventService.HandleEventRelatedCartItemRemovalAsync(customer, cart,itemIdsToRemove);

                            // Check if any cart item contains the special attribute
                            var containsSpecialAttribute = cart.Any(item => _specialOffersService.ContainsSpecialOfferAttribute(item.AttributesXml));

                            if (containsSpecialAttribute)
                            { 
                                // Get order items with changed quantity
                                var itemsWithNewQuantity = cart.Select(item => new
                                {
                                    // Try to get a new quantity for the item, set 0 for items to remove
                                    NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
                                    Item = item,
                                    Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
                                }).Where(item => item.NewQuantity != item.Item.Quantity).ToList();

                                foreach (var cartItem in itemsWithNewQuantity)
                                {
                                    if (cartItem.NewQuantity < cartItem.Item.Quantity)
                                    {
                                        // Handle reduction in quantity
                                        await _specialOffersService.HandleGProductsOnFProductRemovalAsync(cartItem.Item, cart);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var actionExcutedtContext = await next();

            if (controller.ControllerTypeInfo == typeof(ShoppingCartController) && context.HttpContext.Request.Method.ToString() == "POST" &&
                       controller.ActionName.Equals("Cart"))
            {
                //If plugin is enable
                if (settings.IsEnablePlugin)
                {
                    //Check Apply giftcard coupon is clicked
                    if (actionExcutedtContext.HttpContext.Request.Form.Keys.Any(x => x.Equals("applygiftcardcouponcode", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var warningMessage = await _giftCardAdditionalInfoService.ValidateGiftCardAsync(context.ActionArguments["giftcardcouponcode"]?.ToString(), customer);
                        if (!string.IsNullOrEmpty(warningMessage))
                        {
                            var actionExcutedtController = actionExcutedtContext.Controller as Controller;
                            ShoppingCartModel model = actionExcutedtController.ViewData.Model as ShoppingCartModel;
                            if (model != null && model.GiftCardBox.IsApplied)
                            {
                                model.GiftCardBox.IsApplied = false;
                                model.GiftCardBox.Message = warningMessage;
                            }
                        }
                    }

                    //Check Apply voucher coupon is clicked
                    if (actionExcutedtContext.HttpContext.Request.Form.Keys.Any(x => x.Equals("applydiscountcouponcode", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var discountCouponCode = context.ActionArguments["discountcouponcode"]?.ToString();
                        var _discountCustomerMappingService = EngineContext.Current.Resolve<IDiscountCustomerMappingService>();

                        // Get the model from the controller context
                        var actionExcutedtController = actionExcutedtContext.Controller as Controller;
                        var model = actionExcutedtController.ViewData.Model as ShoppingCartModel;

                        await _discountCustomerMappingService.HandleSpecialDiscountCodeApplicationAsync(customer, discountCouponCode, model);
                    }
                }
            }
        }
    }
}