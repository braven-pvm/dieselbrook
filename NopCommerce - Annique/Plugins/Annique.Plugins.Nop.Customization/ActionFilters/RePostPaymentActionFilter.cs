using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class RePostPaymentActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly ILogger _logger;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public RePostPaymentActionFilter(IWorkContext workContext,
            IAddressService addressService,
            ILogger logger,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IOrderService orderService)
        {
            _workContext = workContext;
            _addressService = addressService;
            _logger = logger;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _orderService = orderService;
        }

        #endregion

        #region Method

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            var customer = await _workContext.GetCurrentCustomerAsync();

            // Execute code before the action method
            if (controllerActionDescriptor.ControllerTypeInfo == typeof(OrderController) &&
                           controllerActionDescriptor.ActionName.Equals("Details"))
            {
                try
                {
                    // Check if the request has form content type
                    if (context.HttpContext.Request.HasFormContentType)
                    {
                        //Check repost payment is clicked
                        if (context.HttpContext.Request.Form.Keys.Any(x => x.Equals("repost-payment", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            // Extract orderId from the action arguments
                            if (context.ActionArguments.TryGetValue("orderId", out object orderIdObj) && orderIdObj is int orderId)
                            {
                                // Get the order by orderId
                                var order = await _orderService.GetOrderByIdAsync(orderId);

                                if (order == null || order.Deleted || customer.Id != order.CustomerId)
                                {
                                    // Unauthorized
                                    context.Result = new ChallengeResult();
                                    return;
                                }

                                //check annique plugin enabled or not
                                var isAnniquePluginEnabled = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

                                //if annique plugin not enabled then return so further execution willl not work
                                if (!isAnniquePluginEnabled)
                                    return;

                                //get customer billing address 
                                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                                //Validate and update billing address
                                await _anniqueCustomizationConfigurationService.ValidateBillingAddress(billingAddress);
                            }

                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, customer);
                }
            }

            await next();
        }

        #endregion
    }
}
