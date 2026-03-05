using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class CustomerRegisterActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        #endregion

        #region Ctor

        public CustomerRegisterActionFilter(
            IWorkContext workContext,
            ICustomerService customerService,
            ICustomerModelFactory customerModelFactory,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            ILogger logger,
            ILocalizationService localizationService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _customerModelFactory = customerModelFactory;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _logger = logger;
            _localizationService = localizationService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
        }

        #endregion

        #region Method

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controller.ControllerTypeInfo == typeof(CustomerController) &&
                context.HttpContext.Request.Method.ToString() == "POST" &&
                controller.ActionName.Equals("Register"))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                try
                {
                    var isPluginEnabled = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();
                    if(!isPluginEnabled)
                        return;

                    if (context.ActionArguments.TryGetValue("model", out var modelObj) && modelObj is RegisterModel model)
                    {
                        var phone = model.Phone?.Trim();

                        if (!string.IsNullOrEmpty(phone))
                        {
                            customer.Phone = phone;
                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, customer);
                }
            }

            // Continue to the action method
            var resultContext = await next();

            // Execute code after the action method 
            if (resultContext.Exception == null)
            {
                if (controller.ControllerTypeInfo == typeof(CustomerController) &&
                context.HttpContext.Request.Method.ToString() == "POST" &&
                controller.ActionName.Equals("Register"))
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    try
                    {
                        if (await _anniqueCustomizationConfigurationService.IsPluginEnableAsync())
                        {
                            if (context.ActionArguments.TryGetValue("model", out var modelObj) && modelObj is RegisterModel model)
                            {
                                var controllerContext = (Controller)context.Controller;
                                if (controllerContext.ModelState.IsValid)

                                    #region #611 Client role to user

                                    //set client role if no affiliate
                                    await _anniqueCustomizationConfigurationService.SetClientRoleToUserAsync(customer);

                                    #endregion
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        await _logger.WarningAsync(exc.Message, exc, customer);
                    }
                }
            }
        }

        #endregion
    }
}
