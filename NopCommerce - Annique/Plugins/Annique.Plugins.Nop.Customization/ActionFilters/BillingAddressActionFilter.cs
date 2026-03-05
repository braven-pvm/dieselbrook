using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class BillingAddressActionFilter : IAsyncActionFilter
    {
        #region Fields

        private string _oldBillingAddressId = string.Empty;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly ILogger _logger;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public BillingAddressActionFilter(IWorkContext workContext,
            IAddressService addressService,
            ILogger logger,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _addressService = addressService;
            _logger = logger;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Method

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Execute code before the action method
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controllerActionDescriptor.ControllerTypeInfo == typeof(CheckoutController) &&
                      controllerActionDescriptor.ActionName.Equals("BillingAddress"))
            {
                //get current store
                var store = await _storeContext.GetCurrentStoreAsync();

                //get Active store Annique Settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                //If plugin is disable do not continue
                if (!settings.IsEnablePlugin)
                    return;

                //get current customer
                var customer = await _workContext.GetCurrentCustomerAsync();

                //get old billing address id
                _oldBillingAddressId = customer.BillingAddressId.HasValue ? customer.BillingAddressId.Value.ToString() : "NULL";
            }
            // Continue to the action method
            var resultContext = await next();

            // Execute code after the action method 
            if (resultContext.Exception == null)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (controllerActionDescriptor.ControllerTypeInfo == typeof(CheckoutController) &&
                           controllerActionDescriptor.ActionName.Equals("BillingAddress"))
                {
                    try
                    {
                        //check annique plugin enabled or not
                        var isAnniquePluginEnabled = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();
                        
                        //if annique plugin not enabled then return so further execution willl not work
                        if (!isAnniquePluginEnabled)
                            return;

                        if (customer != null && customer.BillingAddressId.HasValue)
                        {
                            //get customer billing address 
                            var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);

                            //Validate and update billing address
                            await _anniqueCustomizationConfigurationService.ValidateBillingAddress(billingAddress);

                            //check if user has consultant role
                            var isConsultantUser = await _anniqueCustomizationConfigurationService.IsConsultantRoleAsync();
                            if (!isConsultantUser)
                                return;

                            //if user has consultant role and new billing addresss button clicked
                            //then capture new default billing address id into customer changes table

                            // Check if the request has form content type(new billing address clicked)
                            if (context.HttpContext.Request.HasFormContentType)
                            {
                                //Check form key available and OldBillingAddressId is null then only updates customer changes table
                                if (context.HttpContext.Request.Form.Keys.Any(x => x.Equals("nextstep", StringComparison.InvariantCultureIgnoreCase)) && _oldBillingAddressId == "NULL")
                                {
                                    await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.CustomerTable, customer.Id, "BillingAddress_Id", _oldBillingAddressId, customer.BillingAddressId.ToString());
                                }
                            }
                            else
                            { 
                                //when default billing address id is set first time from billing address method NUll to first customer address as billing address captures changes into customer changes table
                                if (_oldBillingAddressId == "NULL")
                                    await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, AnniqueCustomizationDefaults.CustomerTable, customer.Id, "BillingAddress_Id", _oldBillingAddressId, customer.BillingAddressId.ToString());
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
