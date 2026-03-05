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
using Nop.Services.Plugins;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Models;

namespace XcellenceIT.Plugin.ProductRibbons.Components
{
    [ViewComponent(Name = "ProductRibbonsSearchVersion")]
    public class ProductRibbonViewComponent : NopViewComponent
    {
        private readonly IPluginService _pluginService;

        public ProductRibbonViewComponent(IPluginService pluginService)
        {
            _pluginService = pluginService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ProductRibbonVersionModel model = new()
            {
                CompanyUrl = "https://www.nopaccelerate.com/"
            };

            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("XcellenceIT.Plugin.ProductRibbons", LoadPluginsMode.InstalledOnly);
            model.Version = pluginDescriptor.Version;

            return View(model);
        }
    }
}
