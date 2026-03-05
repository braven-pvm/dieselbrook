using Annique.Plugins.Nop.Customization.Factories.UserProfile;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using System;
using System.Linq;

namespace Annique.Plugins.Nop.Customization.ActionFilters
{
    public class AddressEditActionFilter : IActionFilter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IUserProfileAdditionalInfoModelFactory _userProfileAdditionalInfoModelFactory;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public AddressEditActionFilter(
            IWorkContext workContext,
            ICustomerService customerService,
            IUserProfileAdditionalInfoModelFactory userProfileAdditionalInfoModelFactory,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILogger logger)
        {
            _workContext = workContext;
            _customerService = customerService;
            _userProfileAdditionalInfoModelFactory = userProfileAdditionalInfoModelFactory;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _settingService = settingService;
            _storeContext = storeContext;
            _logger = logger;
        }

        #endregion

        #region Methods

        // This method is called before the action method is executed.
        // It's used for preparing and storing consultant user's old default billing or shipping data.
        public async void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = (ControllerActionDescriptor)context.ActionDescriptor;

            // Execute code before the action method
            if (controller.ControllerTypeInfo == typeof(CustomerController) && context.HttpContext.Request.Method.ToString() == "POST" &&
                        controller.ActionName.Equals("AddressEdit"))
            {
                try
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
                    //get customer roles id
                    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

                    //If not consultant user do not continue
                    if (!customerRoleIds.Contains(settings.ConsultantRoleId))
                        return;

                    //check any Address.Id key contains in Form
                    if (context.HttpContext.Request.Form.Keys.Any(x => x.Equals("Address.Id", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        FormCollection form = (FormCollection)context.ActionArguments["form"];

                        //get addressId
                        var addressId = form["Address.Id"].ToString();
                        //default billing address id
                        var defaultBillingAddressId = (customer.BillingAddressId == null) ? 0 : customer.BillingAddressId;
                        //default shipping address id
                        var defaultShippingAddressId = (customer.ShippingAddressId == null) ? 0 : customer.ShippingAddressId;

                        if (customer != null && !string.IsNullOrEmpty(addressId) && int.TryParse(addressId, out var addressIdInt))
                        {
                            //if current address is default billing or shipping address then only copy its old value
                            if (addressIdInt == defaultBillingAddressId || addressIdInt == defaultShippingAddressId)
                            {
                                //get address before updating (old address data)
                                var oldAddress = await _customerService.GetCustomerAddressAsync(customer.Id, addressIdInt);
                                if (oldAddress == null)
                                    return;

                                //prepare address fields
                                var oldAddressCopy = _userProfileAdditionalInfoModelFactory.PrepareOldAddressCopy(oldAddress);

                                //Store the old address copy in context.HttpContext.Items
                                context.HttpContext.Items["OldAddress"] = oldAddressCopy;
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                }
            }
        }

        // This method is called after the action method has executed.
        // It's used for comparing old and new address data and recording changes into ANQ_CustomerChanges table.
        public async void OnActionExecuted(ActionExecutedContext context)
        {
            var controller = (ControllerActionDescriptor)context.ActionDescriptor;

            if (controller.ControllerTypeInfo == typeof(CustomerController) && context.HttpContext.Request.Method.ToString() == "POST" &&
                        controller.ActionName.Equals("AddressEdit"))
            {
                try 
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
                    //get customer roles id
                    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

                    //If not consultant user do not continue
                    if (!customerRoleIds.Contains(settings.ConsultantRoleId))
                        return;

                    //get old address copy from context.HttpContext.Items
                    if (context.HttpContext.Items.TryGetValue("OldAddress", out var oldAddressObj) && oldAddressObj is Address oldAddress)
                    {
                        // Retrieve the new address data from form collection.
                        if (context.HttpContext.Request.Form.TryGetValue("Address.Id", out var newAddressIdValues) &&
                            int.TryParse(newAddressIdValues, out var newAddressIdInt))
                        { 
                            //get new address
                            var newAddress = await _customerService.GetCustomerAddressAsync(customer.Id, newAddressIdInt);

                            if (newAddress != null)
                            {
                                var oldAddressType = oldAddress.GetType();
                                var properties = oldAddressType.GetProperties();

                                string addressType = string.Empty;
                                if (newAddressIdInt == customer.BillingAddressId)
                                {
                                    addressType = AnniqueCustomizationDefaults.DefaultBillingAddressType;
                                }
                                else if (newAddressIdInt == customer.ShippingAddressId)
                                {
                                    addressType = AnniqueCustomizationDefaults.DefaultShippingAddressType;
                                }
                                else
                                {
                                    addressType = AnniqueCustomizationDefaults.AddressTable;
                                }

                                //iterate over the properties of oldAddress
                                foreach (var property in properties)
                                {
                                    var oldPropertyValue = property.GetValue(oldAddress);
                                    var newPropertyValue = property.GetValue(newAddress);

                                    if (!Equals(oldPropertyValue, newPropertyValue))
                                    {
                                        // Call the UpdateCustomerChanges method for each property
                                        var propertyName = property.Name;
                                        if (propertyName == "CustomAttributes" || propertyName == "CreatedOnUtc")
                                            continue;
                                        
                                        // Handle properties that may be null or missing
                                        oldPropertyValue ??= "N/A"; // if null then set N/A as default value
                                        newPropertyValue ??= "N/A"; 

                                        //Update changes in customer changes table
                                        await _userProfileAdditionalInfoService.InsertCustomerChangesAsync(customer, addressType, newAddress.Id, propertyName, oldPropertyValue.ToString().Trim(), newPropertyValue.ToString().Trim());
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    await _logger.WarningAsync(exc.Message, exc,await _workContext.GetCurrentCustomerAsync());
                }
            }
        }
        
        #endregion
    }
}