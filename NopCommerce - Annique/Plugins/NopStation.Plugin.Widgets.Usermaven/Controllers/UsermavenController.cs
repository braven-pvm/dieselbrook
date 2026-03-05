using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Usermaven.Models;

namespace NopStation.Plugin.Widgets.Usermaven.Controllers;

public class UsermavenController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public UsermavenController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(UsermavenPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var usermavenSettings = await _settingService.LoadSettingAsync<UsermavenSettings>(storeScope);

        var model = new ConfigurationModel
        {
            ActiveStoreScopeConfiguration = storeScope,
            EnablePlugin = usermavenSettings.EnablePlugin,
            Script = usermavenSettings.Script
        };

        if (storeScope > 0)
        {
            model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(usermavenSettings, x => x.EnablePlugin, storeScope);
            model.Script_OverrideForStore = await _settingService.SettingExistsAsync(usermavenSettings, x => x.Script, storeScope);
        }

        return View("~/Plugins/NopStation.Plugin.Widgets.Usermaven/Views/Usermaven/Configure.cshtml", model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(UsermavenPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var usermavenSettings = await _settingService.LoadSettingAsync<UsermavenSettings>(storeScope);

        //save settings
        usermavenSettings.EnablePlugin = model.EnablePlugin;
        usermavenSettings.Script = model.Script;

        await _settingService.SaveSettingOverridablePerStoreAsync(usermavenSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(usermavenSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}