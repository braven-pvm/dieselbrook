using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication;
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
    public class CustomerInfoActionFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        #endregion

        #region Ctor

        public CustomerInfoActionFilter(
            IWorkContext workContext,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            ICustomerModelFactory customerModelFactory,
            IAuthenticationService authenticationService,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            ILogger logger,
            ILocalizationService localizationService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
            _customerModelFactory = customerModelFactory;
            _authenticationService = authenticationService;
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
                controller.ActionName.Equals("Info"))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                try
                {
                    var isPluginEnabled = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();
                    if (!isPluginEnabled)
                        return;

                    if (context.ActionArguments.TryGetValue("model", out var modelObj) && modelObj is CustomerInfoModel model)
                    {
                        var controllerContext = (Controller)context.Controller;

                        #region task 610 Format phone number for sendinblue plugin

                        //to format phone number country id is required so setting country id for customer
                        await _userProfileAdditionalInfoService.SetDefaultCountryAsync(customer);
                        model.CountryId = customer.CountryId;

                        #endregion

                        if (!string.IsNullOrWhiteSpace(model.Phone))
                        {
                            var trimmedModelPhone = model.Phone.Trim();
                            var trimmedCustomerPhone = customer.Phone?.Trim();

                            // If phone has changed or customer had no phone before
                            if (!string.Equals(trimmedModelPhone, trimmedCustomerPhone, StringComparison.Ordinal))
                            {
                                var isTaken = await _userProfileAdditionalInfoService.IsPhoneOrWhatsappNumberTakenByOtherAsync(trimmedModelPhone, customer.Id);
                                if (isTaken)
                                    controllerContext.ModelState.AddModelError("", await _localizationService.GetResourceAsync("Annique.Plugin.PhoneNumber.Validation.Message"));
                            }
                        }

                        var email = model.Email.Trim();

                        // Modified string comparison to handle potential null values.
                        // In default nopCommerce, comparing customer.Email with email using
                        // Equals() method can result in a NullReferenceException if customer.Email is null.
                        // The updated code uses string.Equals() which safely handles null values and
                        // performs a case-insensitive comparison using StringComparison.InvariantCultureIgnoreCase.

                        if (!string.Equals(customer.Email, email, StringComparison.InvariantCultureIgnoreCase))
                        {
                            #region Task 600 email verification 

                            // Verify email using API before setting it
                            var isEmailValid = await _userProfileAdditionalInfoService.VerifyEmailByApiAsync(email);
                            if (!isEmailValid)
                            {
                                controllerContext.ModelState.AddModelError("", await _localizationService.GetResourceAsync("Annique.Plugin.EmailableApi.Validation.Message"));

                                // Prepare the model and return the view with the error
                                var customerAttributesXml = context.HttpContext.Request.Form.ToString();
                                model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true, customerAttributesXml);
                                context.Result = controllerContext.View(model);
                                return; // Stop further execution if email is invalid
                            }

                            #endregion

                            var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                            await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                            customer.Username = email;

                            if (_workContext.OriginalCustomerIfImpersonated == null &&
                                !_customerSettings.UsernamesEnabled && !requireValidation)
                            {
                                await _authenticationService.SignInAsync(customer, true);
                            }
                        }

                        await _customerService.UpdateCustomerAsync(customer);
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