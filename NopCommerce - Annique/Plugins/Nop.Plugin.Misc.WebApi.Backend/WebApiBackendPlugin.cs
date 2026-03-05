using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Plugin.Misc.WebApi.Backend.Services.Security;
using Nop.Plugin.Misc.WebApi.Framework;
using Nop.Plugin.Misc.WebApi.Framework.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebApi.Backend
{
    /// <summary>
    /// Represents the Web API Backend plugin
    /// </summary>
    public partial class WebApiBackendPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly WebApiHttpClient _webApiHttpClient;

        #endregion

        #region Ctor

        public WebApiBackendPlugin(IActionContextAccessor actionContextAccessor,
            IJwtTokenService jwtTokenService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            WebApiHttpClient webApiHttpClient)
        {
            _actionContextAccessor = actionContextAccessor;
            _jwtTokenService = jwtTokenService;
            _localizationService = localizationService;
            _permissionService = permissionService;
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
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(WebApiBackendDefaults.ConfigurationRouteName);
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
                ["Plugins.WebApi.Backend.DeveloperMode"] = "Developer mode",
                ["Plugins.WebApi.Backend.DeveloperMode.Hint"] = "Developer mode allows you to make requests without using JWT.",
                ["Plugins.WebApi.Backend.SecretKey"] = "Secret key",
                ["Plugins.WebApi.Backend.SecretKey.Generate"] = "Generate new",
                ["Plugins.WebApi.Backend.SecretKey.Hint"] = "The secret key to sign and verify each JWT token.",
            });

            //settings
            await _settingService.SaveSettingAsync(new WebApiCommonSettings
            {
                TokenLifetimeDays = WebApiCommonDefaults.TokenLifeTime,
                SecretKey = _jwtTokenService.NewSecretKey
            });

            //add permission
            await _permissionService.InstallPermissionsAsync(new WebApiBackendPermissionProvider());

            //plugin installation confirmation
            try
            {
                await _webApiHttpClient.InstallationCompletedAsync(PluginDescriptor);
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
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.WebApi.Backend");

            //settings
            await _settingService.DeleteSettingAsync<WebApiBackendSettings>();

            //delete permission
            var permissionRecord = (await _permissionService.GetAllPermissionRecordsAsync())
                .FirstOrDefault(x => x.SystemName == WebApiBackendPermissionProvider.AccessWebApiBackend.SystemName);
            var listMappingCustomerRolePermissionRecord = await _permissionService.GetMappingByPermissionRecordIdAsync(permissionRecord.Id);
            foreach (var mappingCustomerPermissionRecord in listMappingCustomerRolePermissionRecord)
                await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(
                    mappingCustomerPermissionRecord.PermissionRecordId,
                    mappingCustomerPermissionRecord.CustomerRoleId);

            await _permissionService.DeletePermissionRecordAsync(permissionRecord);

            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            decimal version;
            try
            {
                version = Convert.ToDecimal(currentVersion, CultureInfo.InvariantCulture);
            }
            catch 
            {
               return;
            }
            
            if (version < 1.04M)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
                {
                    ["Plugins.WebApi.Backend.SecretKey.Generate"] = "Generate new"
                });
            }
        }

        #endregion
    }
}
