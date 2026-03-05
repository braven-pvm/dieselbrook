// *************************************************************************
// *                                                                       *
// * Product Ribbons Plugin for nopCommerce                               *
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using XcellenceIt.Core;
using XcellenceIt.Core.Enums;
using XcellenceIT.Plugin.ProductRibbons.ActionFilters;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Models;
using XcellenceIT.Plugin.ProductRibbons.Services;
using XcellenceIT.Plugin.ProductRibbons.Utilities;

namespace XcellenceIT.Plugin.ProductRibbons.Controllers
{
    [Area(AreaNames.Admin)]
    public class ProductRibbonsConfigurationController : BasePluginController
    {
        #region Field

        private readonly ISettingService _settingService;
        private readonly IPluginService _pluginService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IProductRibbonsService _productRibbonsService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public ProductRibbonsConfigurationController(
            ISettingService settingService,
            IPluginService pluginService,
             IStoreContext storeContext,
             ILocalizationService localizationService,
             IWebHelper webHelper,
             IPermissionService permissionService,
             INotificationService notificationService,
             IProductRibbonsService productRibbonsService,
             ISpecificationAttributeService specificationAttributeService,
             IPictureService pictureService)
        {
            _settingService = settingService;
            _pluginService = pluginService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _productRibbonsService = productRibbonsService;
            _specificationAttributeService = specificationAttributeService;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        #region Configure

        public virtual async Task<IActionResult> Configure()
        {
            var model = new ConfigurationModel();

            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            // Get product ribbon setting from store wise 
            var productRibbonSetting = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(storeScope);

            #region License Implementation

            PluginDescriptor pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("XcellenceIT.Plugin.ProductRibbons");
            var buildDate = _productRibbonsService.GetBuildDate(Assembly.GetExecutingAssembly());

            LicenseImplementer licenseImplementer = new LicenseImplementer();
            model.IsLicenseActive = await _productRibbonsService.IsLicenseActiveAsync();
            if (model.IsLicenseActive)
                model.LicenseInformation = await licenseImplementer.GetLicenseInformationAsync(pluginDescriptor.SystemName, productRibbonSetting.LicenseKey, buildDate, ProductName.ProductRibbon);
            else
            {
                string validationURL = string.Empty;
                validationURL = _webHelper.GetStoreLocation();
                if (!_webHelper.GetStoreLocation().EndsWith("/"))
                    validationURL = "/";

                validationURL += "Admin/ProductRibbonsConfiguration/ValidateLicense";

                model.RegistrationForm = licenseImplementer.GetRegistrationForm(ProductName.ProductRibbon,
                    LicenseApiVersion.V1, NopVersion.CURRENT_VERSION, "",
                    validationURL, "http://shop.xcellence-it.com/product-ribbon-plugin");
            }

            #endregion

            //Prepare model

            // Set model class object properties from settings saved into database
            model.ActiveStoreScopeConfiguration = storeScope;

            model.Enabled = productRibbonSetting.XITRibbonEnabled;
            model.CSSProductBoxSelector = productRibbonSetting.CSSProductBoxSelector;
            model.CSSProductPagePicturesParentContainerSelector = productRibbonSetting.CSSProductPagePicturesParentContainerSelector;
            model.SpecificationAttributeId = productRibbonSetting.SpecificationAttributeId;
            model.StockStatusRImage = productRibbonSetting.StockStatusRImage;
            model.StockStatusGImage = productRibbonSetting.StockStatusGImage;
            model.StockStatusBImage = productRibbonSetting.StockStatusBImage;
            model.StockStatusOImage = productRibbonSetting.StockStatusOImage;

            // If any store is selected
            if (storeScope > 0)
            {
                model.CSSProductBoxSelectorForProductRibbons_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.CSSProductBoxSelector, storeScope);
                model.CSSProductPagePicturesParentContainerSelectorForProductRibbons_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.CSSProductPagePicturesParentContainerSelector, storeScope);
                model.SpecificationAttributeId_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.SpecificationAttributeId, storeScope);
                model.StockStatusRImage_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.StockStatusRImage, storeScope);
                model.StockStatusGImage_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.StockStatusGImage, storeScope);
                model.StockStatusBImage_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.StockStatusBImage, storeScope);
                model.StockStatusOImage_OverrideForStore = await _settingService.SettingExistsAsync(productRibbonSetting, x => x.StockStatusOImage, storeScope);
            }

            model.AvailableSpecificationAttributes = (await _specificationAttributeService.GetSpecificationAttributesByGroupIdAsync()).Select(specificationAttribute => new SelectListItem
            {
                Text = specificationAttribute.Name,
                Value = specificationAttribute.Id.ToString()
            }).ToList();

            return View( model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            // Load settings according to current store scope
            var productRibbonSetting = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(storeScope);

            //save settings
            productRibbonSetting.XITRibbonEnabled = model.Enabled;
            productRibbonSetting.CSSProductBoxSelector = model.CSSProductBoxSelector;
            productRibbonSetting.CSSProductPagePicturesParentContainerSelector = model.CSSProductPagePicturesParentContainerSelector;
            productRibbonSetting.SpecificationAttributeId = model.SpecificationAttributeId;
            productRibbonSetting.StockStatusRImage = model.StockStatusRImage;
            productRibbonSetting.StockStatusGImage = model.StockStatusGImage;
            productRibbonSetting.StockStatusBImage = model.StockStatusBImage;
            productRibbonSetting.StockStatusOImage = model.StockStatusOImage;

            if (productRibbonSetting.StockStatusRImage > 0)
                productRibbonSetting.StockStatusRImageUrl = await _pictureService.GetPictureUrlAsync(productRibbonSetting.StockStatusRImage);

            if (productRibbonSetting.StockStatusGImage > 0)
                productRibbonSetting.StockStatusGImageUrl = await _pictureService.GetPictureUrlAsync(productRibbonSetting.StockStatusGImage);

            if (productRibbonSetting.StockStatusBImage > 0)
                productRibbonSetting.StockStatusBImageUrl = await _pictureService.GetPictureUrlAsync(productRibbonSetting.StockStatusBImage);

            if (productRibbonSetting.StockStatusOImage > 0)
                productRibbonSetting.StockStatusOImageUrl = await _pictureService.GetPictureUrlAsync(productRibbonSetting.StockStatusOImage);

            if (model.CSSProductBoxSelectorForProductRibbons_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.CSSProductBoxSelector, storeScope, true);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.CSSProductBoxSelector, storeScope);

            if (model.CSSProductPagePicturesParentContainerSelectorForProductRibbons_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.CSSProductPagePicturesParentContainerSelector, storeScope, true);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.CSSProductPagePicturesParentContainerSelector, storeScope);

            if (model.SpecificationAttributeId_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.SpecificationAttributeId, storeScope, true);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.SpecificationAttributeId, storeScope);

            if (model.StockStatusRImage_OverrideForStore || storeScope == 0)
            {
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusRImage, storeScope, true);
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusRImageUrl, storeScope, true);
            }
            else if (storeScope > 0)
            { 
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusRImage, storeScope);
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusRImageUrl, storeScope);
            }


            if (model.StockStatusGImage_OverrideForStore || storeScope == 0)
            {
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusGImage, storeScope, true);
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusGImageUrl, storeScope, true);
            }
            else if (storeScope > 0)
            {
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusGImage, storeScope);
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusGImageUrl, storeScope);
            }

            if (model.StockStatusBImage_OverrideForStore || storeScope == 0)
            {
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusBImage, storeScope, true);
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusBImageUrl, storeScope, true);
            }
            else if (storeScope > 0)
            {
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusBImage, storeScope);
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusBImageUrl, storeScope);
            }

            if (model.StockStatusOImage_OverrideForStore || storeScope == 0)
            {
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusOImage, storeScope, true);
                await _settingService.SaveSettingAsync(productRibbonSetting, x => x.StockStatusOImageUrl, storeScope, true);
            }
            else if (storeScope > 0)
            {
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusOImage, storeScope);
                await _settingService.DeleteSettingAsync(productRibbonSetting, x => x.StockStatusOImageUrl, storeScope);
            }

            await _settingService.SaveSettingAsync(productRibbonSetting);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
        #endregion

        #region License Check

        /// <summary>
        /// Validate License
        /// </summary>
        /// <param name="licenseKey">licenseKey</param>
        /// <returns></returns>
        [HttpPost]
        [CustomErrorHandler]
        [Area(AreaNames.Admin)]
        public async Task<JsonResult> ValidateLicense(string licenseKey)
        {
            try
            {
                var buildDate = _productRibbonsService.GetBuildDate(Assembly.GetExecutingAssembly());
                PluginDescriptor pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("XcellenceIT.Plugin.ProductRibbons");
                LicenseDetails licenseDetails = new LicenseDetails
                {
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    NopVersion = NopVersion.CURRENT_VERSION,
                    SystemName = pluginDescriptor.SystemName,
                    ProductName = ProductName.ProductRibbon,
                    ProductVersion = pluginDescriptor.Version,
                    Hours = 24,
                    CurrentApiVersion = LicenseApiVersion.V1,
                    LicenseKey = licenseKey,
                    DomainName = _webHelper.GetStoreLocation()
                };

                LicenseImplementer licenseImplementer = new LicenseImplementer();
                LicenseRegistrationStatus registrationStatus = await licenseImplementer.RegisterLicenseKeyAsync(pluginDescriptor.SystemName, licenseDetails.LicenseKey, buildDate);
                if (registrationStatus.ActiveStatus)
                {
                    var currentStore = await _storeContext.GetCurrentStoreAsync();

                    var _nopAccelerateCatalogSettings = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(currentStore.Id);

                    _nopAccelerateCatalogSettings.LicenseKey = registrationStatus.LicenseKey;
                    await _settingService.SaveSettingAsync(_nopAccelerateCatalogSettings, x => x.LicenseKey, 0, true);
                    await _settingService.SaveSettingAsync(_nopAccelerateCatalogSettings, x => x.OtherLicenseSettings, 0, true);


                    return Json(new { status = "success", success = registrationStatus.StatusMessage + Environment.NewLine + "<br/>STATUS: " + registrationStatus.StatusDescription });
                }
                else if (string.IsNullOrEmpty(registrationStatus.StatusDescription))
                    return Json(new { status = "error", error = registrationStatus.StatusMessage });
                else
                    return Json(new
                    {
                        status = "error",
                        error = registrationStatus.StatusMessage +
                    Environment.NewLine + "<br/>STATUS: " + registrationStatus.StatusDescription
                    });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion
    }
}
