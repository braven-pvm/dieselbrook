using Annique.Plugins.Nop.Customization.Models.UserLogin;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that logouts user if no activity upto LoginTimeLimit 
    /// </summary>
    [ViewComponent(Name = "UserLoginTimeout")]
    public class UserLoginTimeoutViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public UserLoginTimeoutViewComponent(IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
             IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService)
        {
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return Content(string.Empty);

            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var customer = await _workContext.GetCurrentCustomerAsync();

            //check current customer is loggedin or not
            var isCurrentUserLoggedIn = await _customerService.IsRegisteredAsync(customer);

            var model = new UserLoginTimeOutModel
            {
                IsUserLoggedIn = isCurrentUserLoggedIn,

                //if no time limit set take 5 as fall back time out minute 
                LoginTimeOutMinutes = (settings.LoginTimeLimit == 0) ? 5 : settings.LoginTimeLimit
            };

            return View(model);
        }

        #endregion
    }
}
