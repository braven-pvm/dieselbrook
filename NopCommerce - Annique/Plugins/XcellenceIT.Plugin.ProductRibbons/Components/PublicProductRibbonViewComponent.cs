// *************************************************************************
// *                                                                       *
// * Product RibbonsPlugin for nopCommerce                                 *
// * Copyright(c) Xcellence-IT.All Rights Reserved.                        *
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
// * http://www.nopaccelerate.com/terms/                                   *
// *                                                                       *
// *************************************************************************
// *************************************************************************

using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Models;
using XcellenceIT.Plugin.ProductRibbons.Services;
using XcellenceIT.Plugin.ProductRibbons.Utilities;

namespace XcellenceIT.Plugin.ProductRibbons.Components
{
    [ViewComponent(Name = "PublicProductRibbon")]
    public class PublicProductRibbonViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IProductRibbonsService _productRibbonsService;


        public PublicProductRibbonViewComponent(
            ISettingService settingService,
            IStoreContext storeContext,
            IProductRibbonsService productRibbonsService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _productRibbonsService = productRibbonsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ConfigurationModel model = new();

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            //Get settings
            var productRibbonSettings = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(currentStore.Id);

            //return if false
            if (productRibbonSettings == null || !productRibbonSettings.XITRibbonEnabled)
                return Content(string.Empty);

            //Check license detail
            if (await _productRibbonsService.IsLicenseActiveAsync() == false)
                return Content(string.Empty);

            model.Enabled = productRibbonSettings.XITRibbonEnabled;
            model.CSSProductBoxSelector = productRibbonSettings.CSSProductBoxSelector;
            model.CSSProductPagePicturesParentContainerSelector = productRibbonSettings.CSSProductPagePicturesParentContainerSelector;

            return View(model);
        }
    }
}
