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
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Factories;
using XcellenceIT.Plugin.ProductRibbons.Models;
using XcellenceIT.Plugin.ProductRibbons.Services;
using XcellenceIT.Plugin.ProductRibbons.Utilities;

namespace XcellenceIT.Plugin.ProductRibbons.Controllers
{
    [Area(AreaNames.Admin)]
    public class ProductRibbonAdminController : BaseAdminController
    {
        #region Fields

        private readonly IProductRibbonsService _productRibbonsService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IProductRibbonFactory _productRibbonFactory;

        #endregion

        #region Ctor

        public ProductRibbonAdminController(IProductRibbonsService productRibbonsService,
            IStoreService storeService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizedEntityService localizedEntityService,
            ILanguageService languageService,
            ILocalizationService localizationService, 
            IProductService productService,
            IPermissionService permissionService,
            INotificationService notificationService, 
            IProductRibbonFactory productRibbonFactory)
        {
            _productRibbonsService = productRibbonsService;
            _storeService = storeService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
            _localizationService = localizationService;
            _productService = productService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _productRibbonFactory = productRibbonFactory;
        }

        #endregion

        #region Utilities

        protected async void UpdateLocales(ProductPictureRibbon productPictureRibbon, ProductRibbonModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productPictureRibbon,
                    x => x.RibbonText,
                    localized.RibbonText,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            // Check license detail
            if (await _productRibbonsService.IsLicenseActiveAsync() == false)
                return RedirectToAction("Configure", "ProductRibbonsConfiguration");

            var model = new ProductRibbonSearchModel();
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare page parameters
            model.SetGridPageSize();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProductRibbonSearchModel searchModel)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productRibbons = await _productRibbonsService.GetAllProductRibbonsAsync(
                                     name: searchModel.RibbonName,
                                     startDateUtc: searchModel.StartDateUtc,
                                     endDateUtc: searchModel.EndDateUtc,
                                     enabled: searchModel.Enabled,
                                     pageIndex: searchModel.Page - 1,
                                     pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new ProductRibbonListModel().PrepareToGrid(searchModel, productRibbons, () =>
            {
                return productRibbons.Select(productRibbon => new ProductRibbonModel
                {
                    Id = productRibbon.Id,
                    RibbonName = productRibbon.RibbonName,
                    StartDateUtc = productRibbon.StartDateUtc,
                    EndDateUtc = productRibbon.EndDateUtc,
                    Enabled = productRibbon.Enabled,
                    DisplayOrder = productRibbon.DisplayOrder,
                });
            });

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //Check license detail
            if (await _productRibbonsService.IsLicenseActiveAsync() == false)
                return RedirectToAction("Configure", "ProductRibbonsConfiguration");

            var model = new ProductRibbonModel();

            // List of Stores
            var availableStores = await _storeService.GetAllStoresAsync();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(),
            }).ToList();
            model.Enabled = true;
            model.ProductPictureRibbon.Enabled = true;

            //add locales for multiple language tabs
            await AddLocalesAsync(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProductRibbonModel model, bool continueEditing)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //Prepare ProductRibbon Model
                var productRibbon = await _productRibbonFactory.PrepareAddEditProductRibbonModel(model);

                //Insert ProductRibbon
                await _productRibbonsService.InsertProductRibbonAsync(productRibbon);

                if (productRibbon.Id > 0)
                {
                    //Prepare productPictureRibbon Model
                    var productPictureRibbon = await _productRibbonFactory.PrepareProductPictureRibbonModel(model, productRibbon.Id);
                    //Insert productPictureRibbon
                    await _productRibbonsService.InsertProductPictureRibbonAsync(productPictureRibbon);

                    UpdateLocales(productPictureRibbon, model);
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Notification.add"));

                if (continueEditing)
                    return RedirectToAction("Edit", new { id = productRibbon.Id });
                else
                    return RedirectToAction("List");

            }

            // List of Stores
            var availableStores = await _storeService.GetAllStoresAsync();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(),
            }).ToList();
            model.Enabled = true;
            model.ProductPictureRibbon.Enabled = true;

            return View(model);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Edit(int id)
        {
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var productRibbonsSettings = _settingService.LoadSettingAsync<ProductRibbonsSettings>(currentStore.Id);

            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //Check license detail
            if (await _productRibbonsService.IsLicenseActiveAsync() == false)
                return RedirectToAction("Configure", "ProductRibbonsConfiguration");

            var productRibbon = await _productRibbonsService.GetProductRibbonByIdAsync(id);
            if (productRibbon == null)
                return RedirectToAction("List");

            var model = await _productRibbonFactory.PrepareEditViewModel(productRibbon);
            var productPictureRibbon = await _productRibbonsService.GetProductPictureRibbonByIdAsync(productRibbon.Id);

            //prepare nested search model
            await _productRibbonFactory.PrepareRibbonProductSearchModel(model.productRibbonSearchModel, productPictureRibbon);
            //Get selected store list
            var allStores = await _storeService.GetAllStoresAsync();

            if (!string.IsNullOrWhiteSpace(productRibbon.StoreIds))
            {
                string[] Store = productRibbon.StoreIds.Split(',').Select(sValue => sValue.Trim()).ToArray();
                List<int> StoreList = (Store != null) ? Array.ConvertAll(Store, int.Parse).ToList() : null;
                foreach (var store in allStores)
                {
                    model.AvailableStores.Add(new SelectListItem
                    {
                        Text = store.Name,
                        Value = store.Id.ToString()
                    });
                    model.StoreList = StoreList;
                }
            }

            //get localized name for multiple language tabs
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.RibbonText = await _localizationService.GetLocalizedAsync(productPictureRibbon, x => x.RibbonText, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProductRibbonModel model, bool continueEditing)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productRibbon = await _productRibbonsService.GetProductRibbonByIdAsync(model.Id);

            if (productRibbon != null)
            {
                productRibbon = await _productRibbonFactory.PrepareAddEditProductRibbonModel(model);

                await _productRibbonsService.UpdateProductRibbonAsync(productRibbon);

                var productPictureRibbon = await _productRibbonsService.GetProductPictureRibbonByIdAsync(productRibbon.Id);
                if (productPictureRibbon != null)
                {
                    productPictureRibbon.RibbonText = model.ProductPictureRibbon.RibbonText;
                    productPictureRibbon.RibbonId = productRibbon.Id;
                    productPictureRibbon.Position = model.ProductPictureRibbon.Position;
                    productPictureRibbon.PictureId = model.ProductPictureRibbon.PictureId;
                    productPictureRibbon.Enabled = model.ProductPictureRibbon.Enabled;
                    productPictureRibbon.ContainerStyleCss = model.ProductPictureRibbon.ContainerStyleCss;
                    productPictureRibbon.ImageStyleCss = model.ProductPictureRibbon.ImageStyleCss;
                    productPictureRibbon.TextStyleCss = model.ProductPictureRibbon.TextStyleCss;

                    await _productRibbonsService.UpdateProductPictureRibbonAsync(productPictureRibbon);

                    // update local values
                    UpdateLocales(productPictureRibbon, model);
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Notification.update"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                //prepare nested search model
                await _productRibbonFactory.PrepareRibbonProductSearchModel(model.productRibbonSearchModel, productPictureRibbon);
                return RedirectToAction("Edit", new { id = productRibbon.Id });
            }

            // List of Stores
            var availableStores = await _storeService.GetAllStoresAsync();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(),
            }).ToList();
            model.Enabled = true;
            model.ProductPictureRibbon.Enabled = true;

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productRibbon = await _productRibbonsService.GetProductRibbonByIdAsync(id);
            if (productRibbon == null)
                return RedirectToAction("List");

            await _productRibbonsService.DeleteProductRibbonAsync(productRibbon);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("XcellenceIT.Plugin.ProductRibbons.Notification.Deleted"));
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> ProductRibbonDelete(int id)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return Content("Access Denied");

            var productRibbon = await _productRibbonsService.GetProductRibbonByIdAsync(id);
            if (productRibbon == null)
                return RedirectToAction("List");

            await _productRibbonsService.DeleteProductRibbonAsync(productRibbon);

            return new NullJsonResult();
        }

        #region Products

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(ProductRibbonSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a category with the specified id
            var productRibbon = await _productRibbonsService.GetProductRibbonByIdAsync(searchModel.RibbonId)
                ?? throw new ArgumentException("No Ribbon found with the specified id");

            //prepare model
            var model = await _productRibbonFactory.PrepareRibbonProductListModelAsync(searchModel, productRibbon);

            return Json(model);
        }

        public virtual async Task<IActionResult> ProductDelete(int id)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var ribbonProductsRecord = await _productRibbonsService.GetProductRibbonMappingByIdAsync(id);
            if (ribbonProductsRecord == null)
                throw new ArgumentException("No product mapping found with the specified id" + id);

            await _productRibbonsService.DeleteProductRibbonMappingAsync(ribbonProductsRecord);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductAddPopup(int ribbonId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = await _productRibbonFactory.PrepareAddProductToRibbonSearchModelAsync(new AddProductToCategorySearchModel());

            return View( model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _productRibbonFactory.PrepareAddProductToRibbonListModelAsync(searchModel);

            return Json(model);

        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductAddPopup(ProductRibbonModel.AddProductMappingModel model)
        {
            //Check Authorization
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (var id in model.SelectedProductIds)
                {
                    var product = await _productService.GetProductByIdAsync(id);
                    if (product != null)
                    {
                        var existingProducts_Tab_Mappings = await _productRibbonsService.GetProductRibbonMappingRibbonIdAsync(model.RibbonId);
                        if (await _productRibbonsService.FindProductRibbonAsync(existingProducts_Tab_Mappings, id, model.RibbonId) == null)
                        {
                            await _productRibbonsService.InsertProductRibbonMappingAsync(
                                new ProductRibbonMapping
                                {
                                    RibbonId = model.RibbonId,
                                    ProductId = id,
                                });
                        }
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(new AddProductToCategorySearchModel());
        }

        #endregion

        #endregion

    }
}
