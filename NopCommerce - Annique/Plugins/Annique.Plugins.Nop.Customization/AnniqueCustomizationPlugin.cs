using Annique.Plugins.Nop.Customization.Components;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Annique.Plugins.Nop.Customization
{
    public class AnniqueCustomizationPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IRepository<Language> _languageRepository;
        private readonly INopFileProvider _nopFileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly WidgetSettings _widgetSettings;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public AnniqueCustomizationPlugin(IRepository<Language> languageRepository,
            INopFileProvider nopFileProvider,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            WidgetSettings widgetSettings,
            IScheduleTaskService scheduleTaskService)
        {
            _languageRepository = languageRepository;
            _nopFileProvider = nopFileProvider;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _widgetSettings = widgetSettings;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Admin Side Menu 

        /// <summary>
        ///Admin side menu
        /// </summary>
        public async Task ManageSiteMapAsync(SiteMapNode siteMapNode)
        {
            var storeUrl = _webHelper.GetStoreLocation();
            // Add Menu to Plugin Menu
            var mainMenuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.Menu.MainTitle"),
                Visible = true,
                IconClass = "nav-icon fab fa-buysellads fa-lg"
            };

            // Add Configure Menu
            var Configure = new SiteMapNode()
            {
                SystemName = "Annique.Plugin.Nop.Customization.Configure",
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.Configure.Tab"),
                ControllerName = "AnniqueCustomization",
                ActionName = "Configure",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugin.Nop.Customization", null } }
            };
            mainMenuItem.ChildNodes.Add(Configure);

            // Add Report menu
            var Report = new SiteMapNode()
            {
                SystemName = "Annique.Plugin.Nop.Customization.Report",
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.Report.Tab"),
                ControllerName = "AdminAnniqueReport",
                ActionName = "List",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugin.Nop.Customization.Report", null } }
            };
            mainMenuItem.ChildNodes.Add(Report);

            // Add shipping menu
            var ShippingRule = new SiteMapNode()
            {
                SystemName = "Annique.Plugin.Nop.Customization.CustomShippingRule",
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.CustomShippingRule.Tab"),
                ControllerName = "AdminCustomShippingRule",
                ActionName = "List",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugin.Nop.Customization.CustomShippingRule", null } }
            };
            mainMenuItem.ChildNodes.Add(ShippingRule);

            // Add feedback menu
            var FeedbackMenu = new SiteMapNode()
            {
                SystemName = "Annique.Plugin.Nop.Customization.ChatFeedback",
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.ChatFeedback.Tab"),
                ControllerName = "AnniqueCustomization",
                ActionName = "FeedbackList",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugin.Nop.Customization.ChatFeedback", null } }
            };
            mainMenuItem.ChildNodes.Add(FeedbackMenu);

            // Add page setting menu
            var PageSettingMenu = new SiteMapNode()
            {
                SystemName = "Annique.Plugin.Nop.Customization.RegistrationPageSettings",
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.RegistrationPageSettings.Tab"),
                ControllerName = "AnniqueCustomization",
                ActionName = "RegistrationPageSetting",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugin.Nop.Customization.RegistrationPageSettings", null } }
            };
            mainMenuItem.ChildNodes.Add(PageSettingMenu);

            // Add page registeration list menu
            var registerListMenu = new SiteMapNode()
            {
                SystemName = "Annique.Plugin.Nop.Customization.ConsultantRegistrationList",
                Title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.ConsultantRegistrationList.Tab"),
                ControllerName = "AnniqueCustomization",
                ActionName = "RegistrationList",
                IconClass = "fa fa-genderless",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Annique.Plugin.Nop.Customization.ConsultantRegistrationList", null } }
            };
            mainMenuItem.ChildNodes.Add(registerListMenu);

            var title = await _localizationService.GetResourceAsync("Annique.Plugin.Nop.Customization.Menu.MainTitle");
            var targetMenu = siteMapNode.ChildNodes.FirstOrDefault(x => x.Title == title);
            if (targetMenu != null)
            {
                targetMenu.ChildNodes.Add(Configure);
            }
            else
                siteMapNode.ChildNodes.Add(mainMenuItem);
        }

        #endregion

        #region Resource string install/uninstall

        /// <summary>
        ///Import Resource string from xml and save
        /// </summary>
        protected virtual async Task InstallLocaleResources()
        {
            //'English' language
            var languages = _languageRepository.Table.Where(l => l.Published).ToList();
            foreach (var language in languages)
            {
                //save resources
                foreach (var filePath in Directory.EnumerateFiles(_nopFileProvider.MapPath("~/Plugins/Annique.Customization/Localization/ResourceString"),
                 "resourceString.nopres.xml", SearchOption.TopDirectoryOnly))
                {
                    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                    using (var streamReader = new StreamReader(filePath))
                    {
                        await localizationService.ImportResourcesFromXmlAsync(language, streamReader);
                    }
                }
            }
        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual async Task DeleteLocalResources()
        {
            var file = Path.Combine(_nopFileProvider.MapPath("~/Plugins/Annique.Customization/Localization/ResourceString"), "resourceString.nopres.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                await _localizationService.DeleteLocaleResourcesAsync(item);
            }
        }

        #endregion

        #region Install Uninstall Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AnniqueCustomization/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //install local resource strings
            await InstallLocaleResources();

            var settings = new AnniqueCustomizationSettings
            {
                IsEnablePlugin = true
            };

            //save settings
            await _settingService.SaveSettingAsync(settings);

            //enable widget
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains("Annique.Customization"))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add("Annique.Customization");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //read all install time sqlscripts and execute the code

            var sqlDataProvider = DataProviderManager.GetDataProvider((DataProviderType)1);
            List<string> lists = new List<string>
                {
                    "AlterSpGetFilterPickUpStores.sql",
                    "DropSpGetPickUpStoreById.sql",
                    "CreateSpGetPickUpStoreById.sql"
                };

            foreach (var list in lists)
            {
                string script = File.ReadAllText("Plugins/Annique.Customization/SqlScript/" + list);
                await sqlDataProvider.ExecuteNonQueryAsync(script);
            }

            //install clear temp html file task
            if (await _scheduleTaskService.GetTaskByTypeAsync(AnniqueCustomizationDefaults.ClearTempHtmlFilesTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Seconds = 604800,
                    Name = AnniqueCustomizationDefaults.ClearTempHtmlFilesTaskName,
                    Type = AnniqueCustomizationDefaults.ClearTempHtmlFilesTask,
                });
            }

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            await DeleteLocalResources();

            await _settingService.DeleteSettingAsync<AnniqueCustomizationSettings>();

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains("Annique.Customization"))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove("Annique.Customization");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //read all uninstall time sqlscripts and execute the code
            var sqlDataProvider = DataProviderManager.GetDataProvider((DataProviderType)1);
            List<string> lists = new List<string>
                {
                    "DropSpGetFilterPickUpStore.sql",
                    "DropSpGetPickUpStoreById.sql"
                };

            foreach (var list in lists)
            {
                string script = File.ReadAllText("Plugins/Annique.Customization/SqlScript/" + list);
                await sqlDataProvider.ExecuteNonQueryAsync(script);
            }

            //schedule task
            var task = await _scheduleTaskService.GetTaskByTypeAsync(AnniqueCustomizationDefaults.ClearTempHtmlFilesTask);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);
            await base.UninstallAsync();
        }

        #endregion

        #region widgetzone methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> {
                AdminWidgetZones.CategoryDetailsBlock,
                AdminWidgetZones.ManufacturerDetailsBlock,
                "customer_account_info_bottom",
                "customer_account_additional_info",
                PublicWidgetZones.HeaderBefore,
                PublicWidgetZones.HeaderAfter
               });
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if(widgetZone == AdminWidgetZones.CategoryDetailsBlock)
                return typeof(AdminCategoryTabInfoViewComponent);

            if (widgetZone.Equals(AdminWidgetZones.ManufacturerDetailsBlock))
                return typeof(AdminManufacturerTabInfoViewComponent);

            if (widgetZone.Equals("customer_account_info_bottom"))
                return typeof(CustomersAffiliateInfoViewComponent);

            if (widgetZone.Equals("customer_account_additional_info"))
                return typeof(UserProfileAdditionalInfoViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.HeaderBefore))
                return typeof(UserInfoHeaderBarViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.HeaderAfter))
                return typeof(ActiveSpecialOfferMarqueeViewComponent);

            return typeof(PostNetStoreDeliveryViewComponent);
        }

        public bool HideInWidgetList => false;

        #endregion
    }
}