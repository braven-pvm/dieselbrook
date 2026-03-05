// *************************************************************************
// *                                                                       *
// * Product Ribbons Plugin for nopCommerce                                *
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
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Factories;
using XcellenceIT.Plugin.ProductRibbons.Services;
using XcellenceIT.Plugin.ProductRibbons.Utilities;

namespace XcellenceIT.Plugin.ProductRibbons.Controllers
{
    public class ProductRibbonsPublicController : BasePublicController
    {
        #region Fields

        private readonly IProductRibbonsService _productRibbonsService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IProductRibbonPublicFactory _productRibbonPublicFactory;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public ProductRibbonsPublicController(IProductRibbonsService productRibbonsService,
            IProductService productService,
            ISettingService settingService,
            IStoreContext storeContext,
            IProductRibbonPublicFactory productRibbonPublicFactory,
            ILogger logger,
            IWorkContext workContext)
        {
            _productRibbonsService = productRibbonsService;
            _productService = productService;
            _settingService = settingService;
            _storeContext = storeContext;
            _productRibbonPublicFactory = productRibbonPublicFactory;
            _logger = logger;
            _workContext = workContext;
        }

        #endregion

        #region Utility

        private async Task<Dictionary<int, ProductPictureRibbon>> GetProductPicturesAsync(IList<ProductRibbonRecord> ribbonList)
        {
            var productPictures = new Dictionary<int, ProductPictureRibbon>();

            foreach (var ribbon in ribbonList)
            {
                var productPicture = await _productRibbonsService.GetProductPictureRibbonIdAsync(ribbon.Id);
                productPictures.Add(ribbon.Id, productPicture);
            }

            return productPictures;
        }

        #endregion

        #region Methods 

        [HttpPost]
        public async Task<IActionResult> RetrieveProductsRibbons(int[] productIds)
        {
            try
            {
                Dictionary<int, string> productRibbonDictionary = new Dictionary<int, string>();

                if(!productIds.Any())
                    return Json(productRibbonDictionary);

                // Make productIds distinct
                productIds = productIds.Distinct().ToArray();

                var currentStore = await _storeContext.GetCurrentStoreAsync();

                var productRibbonSettings = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(currentStore.Id);

                #region License and product Ribbon setting validation

                //Check license detail
                if (await _productRibbonsService.IsLicenseActiveAsync() == false)
                    return Json(productRibbonDictionary);

                // return if false
                if (productRibbonSettings != null ? !productRibbonSettings.XITRibbonEnabled : true)
                    return Json(productRibbonDictionary);

                #endregion

                //ribbon list
                var ribbonList = await _productRibbonsService.GetAllEnabledProductRibbonsAsync();

                //dict of ribbon , productPicture
                var productPictures = await GetProductPicturesAsync(ribbonList);

                var products = await _productService.GetProductsByIdsAsync(productIds);
                foreach (var product in products)
                {
                    if (product == null)
                        continue;

                    var productRibbon = await _productRibbonPublicFactory.PreparedRibbon(product, ribbonList, productPictures);
                    productRibbonDictionary.Add(product.Id, await RenderPartialViewToStringAsync("RetrieveProductRibbons", productRibbon));
                }

                return Json(productRibbonDictionary);

            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }
        #endregion

    }
}
