using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Web.Controllers;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class PaymentInfoActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public PaymentInfoActionFilter(IWorkContext workContext,
            IStoreContext storeContext, 
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            ISettingService settingService,
            IHttpContextAccessor httpContextAccessor)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _settingService = settingService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Method

        //task 645 checkout steps , action filter to skip payment info step from multistep checkout process if payment method is pay U
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controllerActionDescriptor.ControllerTypeInfo == typeof(CheckoutController) &&
                 context.HttpContext.Request.Method.ToString() == "GET" &&
                       controllerActionDescriptor.ActionName.Equals("PaymentInfo"))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();

                //get Active store Annique Settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                try
                {
                    if (settings.IsEnablePlugin)
                    {
                        var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
                               customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);

                        //skip payment info step from multistep checkout process if payment method is pay U
                        if (paymentMethodSystemName?.Equals("Atluz.PayUSouthAfrica", StringComparison.InvariantCultureIgnoreCase) == true)
                        { 
                            // create payment info and store in session
                            var paymentInfo = new ProcessPaymentRequest();
                            var serialized = JsonConvert.SerializeObject(paymentInfo);

                            var httpContext = _httpContextAccessor.HttpContext;
                            httpContext?.Session.SetString("OrderPaymentInfo", serialized);

                            //redirect to confirm step
                            context.Result = new RedirectToRouteResult("CheckoutConfirm", null);
                            return;
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
