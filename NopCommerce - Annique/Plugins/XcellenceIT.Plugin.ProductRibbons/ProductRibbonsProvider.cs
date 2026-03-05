// *************************************************************************
// *                                                                       *
// * Product Ribbons  Plugin for nopCommerce                               *
// * Copyright (c) Xcellence-IT. All Rights Reserved.                      *
// *                                                                       *
// *************************************************************************
// *                                                                       *
// * Email: info@nopaccelerate.com                                         *
// * Website: http://www.nopaccelerate.com                                 *
// *                                                                       *
// *************************************************************************
// *                                                                       *
// * This  software is furnished  under a license  and  may  be  used  and *
// * modified  only in  accordance with the terms of such license and with *
// * the  inclusion of the above  copyright notice.  This software or  any *
// * other copies thereof may not be provided or  otherwise made available *
// * to any  other  person.   No title to and ownership of the software is *
// * hereby transferred.                                                   *
// *                                                                       *
// * You may not reverse  engineer, decompile, defeat  license  encryption *
// * mechanisms  or  disassemble this software product or software product *
// * license.  Xcellence-IT may terminate this license if you don't comply *
// * with  any  of  the  terms and conditions set forth in  our  end  user *
// * license agreement (EULA).  In such event,  licensee  agrees to return *
// * licensor  or destroy  all copies of software  upon termination of the *
// * license.                                                              *
// *                                                                       *
// * Please see the  License file for the full End User License Agreement. *
// * The  complete license agreement is also available on  our  website at * 
// * http://www.nopaccelerate.com/enterprise-license                       *
// *                                                                       *
// *************************************************************************
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XcellenceIT.Plugin.ProductRibbons.Components;
using XcellenceIT.Plugin.ProductRibbons.Utilities;

namespace XcellenceIT.Plugin.ProductRibbons
{
    public class ProductRibbonsProvider : BasePlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields 

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IPermissionService _permissionService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public ProductRibbonsProvider(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            INopFileProvider nopFileProvider,
            IPermissionService permissionService,
            WidgetSettings widgetSettings,
            IWidgetPluginManager widgetPluginManager,
            ILanguageService languageService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _nopFileProvider = nopFileProvider;
            _permissionService = permissionService;
            _widgetSettings = widgetSettings;
            _widgetPluginManager = widgetPluginManager;
            _languageService = languageService;
        }

        #endregion

        #region Utilities

        /// <summary>
        ///install Resources
        /// </summary>      
        protected virtual async Task InstallLocaleResourcesAsync()
        {
            var allLanguages = await _languageService.GetAllLanguagesAsync(showHidden: true);
            var directoryPath = _nopFileProvider.MapPath("~/Plugins/XcellenceIT.ProductRibbons/Localization/ResourceString/");
            foreach (var language in allLanguages)
            {
                foreach (var filePath in _nopFileProvider.EnumerateFiles(directoryPath, "*.xml"))
                {
                    using var reader = new StreamReader(filePath);
                    await _localizationService.ImportResourcesFromXmlAsync(language, reader);
                }
            }
        }

        /// <summary>
        ///Uninstall Resources
        /// </summary>       
        protected virtual async Task DeleteLocalResourcesAsync()
        {
            var file = Path.Combine(_nopFileProvider.MapPath("~/Plugins/XcellenceIT.ProductRibbons/Localization/ResourceString"), "EN.ResourceString.xml");

            var language = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                           select name.Attribute("Name").Value;
            foreach (var item in language)
            {
                await _localizationService.DeleteLocaleResourceAsync(item);
            }
        }

        #endregion

        #region Sitemap

        /// <summary>
        /// Authenticate
        /// </summary>
        /// <returns>admin authentications</returns>
        public async Task<bool> Authenticate()
        {
            bool flag = false;
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        // Main menu method to add in plugin menu
        public async Task ManageSiteMapAsync(SiteMapNode siteMapNode)
        {
            // Add ProductRibbons Menu to Plugin Menu
            var mainMenuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.MainTitle.nopAccelerate"),
                Visible = await Authenticate(),
                IconClass = "fab fa-buysellads"
            };

            //Add the pluginmenuitem to plugin sub menu
            var pluginMenuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Sitemap.MainTitle"),
                Visible = await Authenticate(),
                IconClass = "far fa-dot-circle"
            };
            mainMenuItem.ChildNodes.Add(pluginMenuItem);

            var title = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.MainTitle.nopAccelerate");
            var targetMenu = siteMapNode.ChildNodes.FirstOrDefault(x => x.Title == title);
            if (targetMenu != null)
                targetMenu.ChildNodes.Add(pluginMenuItem);
            else
                siteMapNode.ChildNodes.Add(mainMenuItem);

            // configuration page
            var Configure = new SiteMapNode()
            {
                SystemName = "XcellenceIT.Plugin.ProductRibbons",
                Title = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Sitemap.Configure"),
                ControllerName = "ProductRibbonsConfiguration",
                ActionName = "Configure",
                Visible = await Authenticate(),
                IconClass = "far fa-circle",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
            };
            pluginMenuItem.ChildNodes.Add(Configure);

            // Ribbon list page
            var productRibbon = new SiteMapNode()
            {
                SystemName = "Plugin.ProductRibbons.List",
                Title = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Sitemap.RibbonList"),
                ControllerName = "ProductRibbonAdmin",
                ActionName = "List",
                Visible = await Authenticate(),
                IconClass = "far fa-circle",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
            };
            pluginMenuItem.ChildNodes.Add(productRibbon);

            //Get url from setting
            var settings = await _settingService.LoadSettingAsync<ProductRibbonsSettings>();

            // Help document menu
            var HelpDocument = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Sitemap.HelpDocument"),
                Url = settings.HelpDocumentURL,
                OpenUrlInNewTab = true,
                Visible = await Authenticate(),
                IconClass = "far fa-circle",
            };
            pluginMenuItem.ChildNodes.Add(HelpDocument);
        }

        #endregion

        #region Install/Uninstall methods

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            #region Plugin Resources

            await InstallLocaleResourcesAsync();

            #endregion

            var settings = new ProductRibbonsSettings()
            {
                XITRibbonEnabled = false,
                CSSProductBoxSelector = ".product-item .picture",
                CSSProductPagePicturesParentContainerSelector = ".product-essential .picture",
                HelpDocumentURL = "http://docs.nopaccelerate.com/files/product-ribbon-plug-in/"
            };
            //Add ProductRibbons Widget in Active Widgets 
            var widget = await _widgetPluginManager.LoadActivePluginsAsync();

            if (widget.Where(x => x.PluginDescriptor.SystemName == "XcellenceIT.Plugin.ProductRibbons").Any() == false)
            {
                _widgetSettings.ActiveWidgetSystemNames.Add("XcellenceIT.Plugin.ProductRibbons");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.SaveSettingAsync(settings);

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            #region Plugin Resources

            await DeleteLocalResourcesAsync();

            #endregion Plugin Resources

            //Remove  Widget From Active Widgets 
            var widget = await _widgetPluginManager.LoadActivePluginsAsync();

            if (widget.Where(x => x.PluginDescriptor.SystemName == "XcellenceIT.Plugin.ProductRibbons").Any())
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove("XcellenceIT.Plugin.ProductRibbons");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            //Delete all settings
            await _settingService.DeleteSettingAsync<ProductRibbonsSettings>();
            await _settingService.ClearCacheAsync();

            await base.UninstallAsync();
        }

        #endregion

        #region Widget Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            //get list of widget zones name which are selected for Ribbon
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.ContentBefore });
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "PublicProductRibbon";
        }

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ProductRibbonsConfiguration/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone is null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(PublicWidgetZones.ContentBefore))
            {
                return typeof(PublicProductRibbonViewComponent);
            }
            return null;
        }

        #endregion
    }
}
