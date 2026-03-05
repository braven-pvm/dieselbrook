using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.WebApi.Frontend
{
    /// <summary>
    /// Represents the Web API frontend plugin
    /// </summary>
    public partial class WebApiFrontendPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly WebApiHttpClient _webApiHttpClient;

        #endregion

        #region Ctor

        public WebApiFrontendPlugin(IActionContextAccessor actionContextAccessor,
            IJwtTokenService jwtTokenService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            WebApiHttpClient webApiHttpClient)
        {
            _actionContextAccessor = actionContextAccessor;
            _jwtTokenService = jwtTokenService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webApiHttpClient = webApiHttpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(WebApiFrontendDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.WebApi.Frontend.DeveloperMode"] = "Developer mode",
                ["Plugins.WebApi.Frontend.DeveloperMode.Hint"] = "Developer mode allows you to make requests without using JWT.",
                ["Plugins.WebApi.Frontend.SecretKey"] = "Secret key",
                ["Plugins.WebApi.Frontend.SecretKey.Generate"] = "Generate new",
                ["Plugins.WebApi.Frontend.SecretKey.Hint"] = "The secret key to sign and verify each JWT token.",
            });

            //settings
            await _settingService.SaveSettingAsync(new WebApiCommonSettings
            {
                TokenLifetimeDays = WebApiCommonDefaults.TokenLifeTime,
                SecretKey = _jwtTokenService.NewSecretKey
            });

            //plugin installation confirmation
            try
            {
                var response = await _webApiHttpClient.InstallationCompletedAsync(PluginDescriptor);
            }
            catch
            {
                // ignored
            }
            
            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.WebApi.Frontend");

            //settings
            await _settingService.DeleteSettingAsync<WebApiFrontendSettings>();

            await base.UninstallAsync();
        }

        #endregion
    }
}
