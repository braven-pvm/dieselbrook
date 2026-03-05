using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Usermaven.Components;

namespace NopStation.Plugin.Widgets.Usermaven;

public class UsermavenPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
{
    private readonly ILocalizationService _localizationService;
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly IPermissionService _permissionService;
    private readonly INopStationCoreService _nopStationCoreService;

    #endregion

    #region Ctor

    public UsermavenPlugin(ILocalizationService localizationService,
        IWebHelper webHelper,
        ISettingService settingService,
        IPermissionService permissionService,
        INopStationCoreService nopStationCoreService)
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
        _settingService = settingService;
        _permissionService = permissionService;
        _nopStationCoreService = nopStationCoreService;
    }

    #endregion

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HeadHtmlTag
        });
    }

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/Usermaven/Configure";
    }

    public bool HideInWidgetList => false;

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync(new UsermavenPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<UsermavenSettings>();

        await this.UninstallPluginAsync(new UsermavenPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Configuration.Instructions", "<p>Collect javascript tracking code from <a href=\"https://app.usermaven.com/\" target=\"_blank\">Usermaven</a>.</p>"),

            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Configuration.Fields.EnablePlugin", "Enable plugin"),
            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Configuration.Fields.Script", "Script"),
            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Configuration.Fields.Script.Hint", "This field for script."),

            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Configuration", "Usermaven analytics settings"),
            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Menu.Configuration", "Configuration"),
            new KeyValuePair<string, string>("Admin.NopStation.Usermaven.Menu.Usermaven", "Usermaven analytics"),
        };
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Usermaven.Menu.Usermaven"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(UsermavenPermissionProvider.ManageConfiguration))
        {
            var conItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Usermaven.Menu.Configuration"),
                Url = "~/Admin/Usermaven/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Usermaven.Configuration"
            };
            menuItem.ChildNodes.Add(conItem);
        }
        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/usermaven-analytics-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=usermaven-analytics",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);
        }

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
    }

    Type IWidgetPlugin.GetWidgetViewComponent(string widgetZone)
    {
        return typeof(UsermavenViewComponent);
    }
}