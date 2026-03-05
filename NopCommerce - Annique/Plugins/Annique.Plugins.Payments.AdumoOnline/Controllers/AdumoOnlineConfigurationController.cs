using Annique.Plugins.Payments.AdumoOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using System.Threading.Tasks;

namespace Annique.Plugins.Payments.AdumoOnline.Controllers
{
    public class AdumoOnlineConfigurationController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public AdumoOnlineConfigurationController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
        }

        #endregion

        #region Configure

        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AdumoOnlineSettings>(storeScope);
            var model = new ConfigurationModel
            {
                IsEnablePlugin = settings.IsEnablePlugin,
                ActiveStoreScopeConfiguration = storeScope,
                FormPostUrl = settings.FormPostUrl,
                MerchantId = settings.MerchantId,
                ApplicationId = settings.ApplicationId,
                Secret = settings.Secret,
                AdditionalFee= settings.AdditionalFee,
                AdditionalFeePercentage= settings.AdditionalFeePercentage
            };

            if (storeScope > 0)
            {
                model.IsEnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsEnablePlugin, storeScope);
                model.FormPostUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FormPostUrl, storeScope);
                model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MerchantId, storeScope);
                model.ApplicationId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ApplicationId, storeScope);
                model.Secret_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Secret, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AdditionalFeePercentage, storeScope);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AdumoOnlineSettings>(storeScope);

            if (ModelState.IsValid)
            {
                bool appRestart = false;

                if (settings.IsEnablePlugin != model.IsEnablePlugin)
                    appRestart = true;

                settings.IsEnablePlugin = model.IsEnablePlugin;
                settings.MerchantId = model.MerchantId;
                settings.ApplicationId = model.ApplicationId;
                settings.Secret = model.Secret;
                settings.FormPostUrl = model.FormPostUrl;
                settings.AdditionalFee = model.AdditionalFee;
                settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsEnablePlugin, model.IsEnablePlugin_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.FormPostUrl, model.FormPostUrl_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ApplicationId, model.ApplicationId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Secret, model.Secret_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
                
                //now clear settings cache
                await _settingService.ClearCacheAsync();

                if (appRestart)
                {
                    _webHelper.RestartAppDomain();
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

                return await Configure();
            }
            return View(model);
        }

        #endregion
    }
}
