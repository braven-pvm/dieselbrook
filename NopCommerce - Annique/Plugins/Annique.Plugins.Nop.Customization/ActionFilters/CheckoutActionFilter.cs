using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.NewActivityLogs;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class CheckoutActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IGiftService _giftService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILogger _logger;
        private readonly IAdditionalActivityLogService  _additionalActivityLogService;

        #endregion

        #region Ctor

        public CheckoutActionFilter(IStoreContext storeContext,
            ISettingService settingService,
            IWorkContext workContext,
            ICustomerService customerService,
            IGiftService giftService,
            IShoppingCartService shoppingCartService,
            ILogger logger,
            IAdditionalActivityLogService additionalActivityLogService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _workContext = workContext;
            _customerService = customerService;
            _giftService = giftService;
            _shoppingCartService = shoppingCartService;
            _logger = logger;
            _additionalActivityLogService = additionalActivityLogService;
        }

        #endregion

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controllerActionDescriptor.ControllerTypeInfo == typeof(CheckoutController) &&
                       controllerActionDescriptor.ActionName.Equals("Index"))
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                var customer = await _workContext.GetCurrentCustomerAsync();

                //get Active store Annique Settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                try
                {
                    //If plugin is enable
                    if (settings.IsEnablePlugin)
                    {
                        #region Task 644 new Activity logs

                        await _additionalActivityLogService.InsertActivityTrackingLogAsync("PublicStore.CheckoutBegin", "Checkout begin", customer);

                        #endregion

                        var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
                        
                        //Customer current cart
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                        //check customer role contains consultant customer
                        if (customerRoleIds.Contains(settings.ConsultantRoleId))
                        {
                            await _giftService.ProcessGiftsAsync(cart, customer);
                        }

                        #region Special Offers

                        var _specialOffersService = EngineContext.Current.Resolve<ISpecialOffersService>();
                        await _specialOffersService.ValidateAndAdjustGProductQtyInCartAsync(cart, customer);

                        #endregion
                    }

                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, customer);
                }
            }

            await next();
        }
    }
}
