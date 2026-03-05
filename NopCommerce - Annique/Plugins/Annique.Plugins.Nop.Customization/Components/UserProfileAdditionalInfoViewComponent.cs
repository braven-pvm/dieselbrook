using Annique.Plugins.Nop.Customization.Factories.UserProfile;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays Customer Additional Info 
    /// </summary>
    [ViewComponent(Name = "UserProfileAdditionalInfo")]
    public class UserProfileAdditionalInfoViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IUserProfileAdditionalInfoModelFactory _userProfileAdditionalInfoModelFactory;
        private readonly ILogger _logger;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public UserProfileAdditionalInfoViewComponent(IWorkContext workContext,
            IUserProfileAdditionalInfoModelFactory userProfileAdditionalInfoModelFactory,
            ILogger logger,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            ICustomerService customerService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _userProfileAdditionalInfoModelFactory = userProfileAdditionalInfoModelFactory;
            _logger = logger;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _customerService = customerService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Method

        private (bool IsConsultant, bool IsClientOrCustomer) DetermineRoles(IList<CustomerRole> roles, AnniqueCustomizationSettings settings)
        {
            var systemNames = roles.Select(r => r.SystemName).ToList();

            bool isConsultant = roles.Any(r => r.Id == settings.ConsultantRoleId);
            bool isClientOrCustomer = systemNames.Contains("Customer") || systemNames.Contains("Client");

            return (
                IsConsultant: isConsultant,
                IsClientOrCustomer: isClientOrCustomer
            );
        }


        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            try
            {
               var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

                if (!pluginEnable)
                    return Content(string.Empty);

                var store = await _storeContext.GetCurrentStoreAsync();

                // Get store-specific settings (consultant role ID)
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                //Get current customer
                var customer = await _workContext.GetCurrentCustomerAsync();

                var customerRoles = await _customerService.GetCustomerRolesAsync(customer);

                var (isConsultant, isClientOrCustomer) = DetermineRoles(customerRoles,settings);

                //check consultant role exist or not 
                if (isConsultant || isClientOrCustomer)
                {
                    var model = await _userProfileAdditionalInfoModelFactory.PrepareUserProfileAdditionalInfoModelAsync(customer.Id);
                    model.IsConsultant = isConsultant;
                    model.IsClientOrCustomer = isClientOrCustomer;
                    return View(model);
                }

                return Content(string.Empty);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Content(string.Empty);
            }
        }

        #endregion
    }
}
