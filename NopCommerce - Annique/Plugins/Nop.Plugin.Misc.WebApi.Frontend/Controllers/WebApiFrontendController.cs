using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Services;
using Nop.Plugin.Misc.WebApi.Frontend.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public partial class WebApiFrontendController : BasePluginController
    {
        #region Fields

        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor 

        public WebApiFrontendController(IJwtTokenService jwtTokenService,
        ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _jwtTokenService = jwtTokenService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //load settings for active store scope
            var webApiSettings = await _settingService.LoadSettingAsync<WebApiCommonSettings>();

            //prepare model
            var model = new ConfigurationModel
            {
                DeveloperMode = webApiSettings.DeveloperMode,
                SecretKey = webApiSettings.SecretKey
            };

            return View("~/Plugins/Misc.WebApi.Frontend/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for active store scope
            var webApiSettings = await _settingService.LoadSettingAsync<WebApiCommonSettings>();

            //set settings
            webApiSettings.DeveloperMode = model.DeveloperMode;
            webApiSettings.SecretKey = model.SecretKey;

            await _settingService.SaveSettingAsync(webApiSettings, settings => settings.DeveloperMode, clearCache: false);
            await _settingService.SaveSettingAsync(webApiSettings, settings => settings.SecretKey, clearCache: false);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost]
        public virtual IActionResult Generate()
        {
            return Ok(_jwtTokenService.NewSecretKey);
        }

        #endregion
    }
}
