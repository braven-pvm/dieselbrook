using Annique.Plugins.Nop.Customization.Services.CheckoutGifts;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.GiftCardAllocation;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class AdminOrderActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IGiftService _giftService;
        private readonly IAwardService _awardService;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly IGiftCardAdditionalInfoService _giftCardAdditionalInfoService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public AdminOrderActionFilter(
            IStoreContext storeContext,
            ISettingService settingService,
            IWorkContext workContext,
            ILogger logger,
            IOrderService orderService,
            IGiftService giftService,
            IAwardService awardService,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            IGiftCardAdditionalInfoService giftCardAdditionalInfoService,
            ICustomerService customerService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _workContext = workContext;
            _logger = logger;
            _orderService = orderService;
            _giftService = giftService;
            _awardService = awardService;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _giftCardAdditionalInfoService = giftCardAdditionalInfoService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            var actionExcutedtContext = await next();

            var customer = await _workContext.GetCurrentCustomerAsync();

            //when admin manually release order from admin side then process gifts cards , gifts taken , award taken and activation date for consultant user
            if (controllerActionDescriptor.ControllerTypeInfo == typeof(OrderController) && context.HttpContext.Request.Method.ToString() == "POST" &&
                       controllerActionDescriptor.ActionName.Equals("Edit"))
            {
                #region task 618 Gift Items been ordered but not recorded in Gifts Taken 

                try
                {
                    // Check if the request has form content type
                    if (context.HttpContext.Request.HasFormContentType)
                    {
                        //Check mark order as paid or save order status clicked
                        if (actionExcutedtContext.HttpContext.Request.Form.Keys.Any(x => x.Equals("markorderaspaid", StringComparison.InvariantCultureIgnoreCase) || x.Equals("btnSaveOrderStatus", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            //get active store
                            var storeScope = await _storeContext.GetCurrentStoreAsync();

                            //get Active store Annique Settings
                            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

                            //If plugin is disable do not continue
                            if (!settings.IsEnablePlugin)
                                return;

                            // Extract orderId from the action arguments
                            if (context.ActionArguments.TryGetValue("id", out object orderIdObj) && orderIdObj is int orderId)
                            {
                                var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
                                if (!customerRoleIds.Contains(settings.ConsultantRoleId))
                                    return;

                                var order = await _orderService.GetOrderByIdAsync(orderId);
                                if (order == null || order.PaymentStatus != PaymentStatus.Paid)
                                    return;

                                //Get order items
                                var orderItems = await _orderService.GetOrderItemsAsync(orderId);

                                await _giftCardAdditionalInfoService.ProcessGiftcardUsageOnCheckoutAsync(order);

                                await _giftService.ProcessGiftsTakenAsync(order.CustomerId, orderItems);

                                await _userProfileAdditionalInfoService.UpdateActivationDateOnFirstOrderAsync(order);

                                await _awardService.ProcessAwardsTakenAsync(orderItems);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, customer);
                }

                #endregion
            }
        }

        #endregion
    }
}
