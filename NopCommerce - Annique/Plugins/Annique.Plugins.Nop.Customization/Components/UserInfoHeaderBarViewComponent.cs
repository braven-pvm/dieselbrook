using Annique.Plugins.Nop.Customization.Models.UserLogin;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays user info in mobile view
    /// </summary>
    [ViewComponent(Name = "UserInfoHeaderBar")]
    public class UserInfoHeaderBarViewComponent : ViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISettingService _settingService;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;

        #endregion

        #region Ctor

        public UserInfoHeaderBarViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            IStaticCacheManager staticCacheManager,
            ISettingService settingService,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _settingService = settingService;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
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
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!widgetZone?.Equals(PublicWidgetZones.HeaderBefore, StringComparison.InvariantCultureIgnoreCase) ?? true)
                return Content(string.Empty);

            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);
            if (settings != null && !settings.IsEnablePlugin)
                Content(string.Empty);

            //Get current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            var model = new UserInfoHeaderLinksModel();

            var key = _staticCacheManager.PrepareKeyForShortTermCache(AnniqueCustomizationDefaults.CustomerInfoCacheKey, customer);
            model.CustomerFullName = await _staticCacheManager.GetAsync(key, async () => await _anniqueCustomizationConfigurationService.GetCustomizedCustomerFullNameAsync(customer));
            
            if(!string.IsNullOrEmpty(model.CustomerFullName))
                return View(model);

            return Content(string.Empty);
        }

        #endregion
    }
}
