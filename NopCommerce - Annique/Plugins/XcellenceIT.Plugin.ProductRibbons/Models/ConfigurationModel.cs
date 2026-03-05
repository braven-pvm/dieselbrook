// *************************************************************************
// *                                                                       *
// * Product Ribbon Plugin for nopCommerce                                 *
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

using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XcellenceIT.Plugin.ProductRibbons.Models
{
    public record ConfigurationModel : BaseNopEntityModel
    {
        /// <summary>
        /// Gets or Sets the value for Current Store
        /// </summary>
        public int ActiveStoreScopeConfiguration { get; set; }

        /// <summary>
        /// Gets or Sets the value for plugin enabled or not
        /// </summary>
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.Enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// get or set css selector for display product Ribbons on product details page
        /// </summary>
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.CSSProductBoxSelector")]
        public string CSSProductBoxSelector { get; set; }
        public bool CSSProductBoxSelectorForProductRibbons_OverrideForStore { get; set; }

        /// <summary>
        /// get or set css selector for display product Ribbons point on product details page
        /// </summary>
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.CSSProductPagePicturesParentContainerSelector")]
        public string CSSProductPagePicturesParentContainerSelector { get; set; }
        public bool CSSProductPagePicturesParentContainerSelectorForProductRibbons_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets a Specification Attribute Id
        /// </summary>
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.SpecificationAttributeId")]
        public int SpecificationAttributeId { get; set; }
        public bool SpecificationAttributeId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableSpecificationAttributes { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status R
        /// </summary>
        [UIHint("Picture")]
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.StockStatusRImage")]
        public int StockStatusRImage { get; set; }
        public bool StockStatusRImage_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status G
        /// </summary>
        [UIHint("Picture")]
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.StockStatusGImage")]
        public int StockStatusGImage { get; set; }
        public bool StockStatusGImage_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status B
        /// </summary>
        [UIHint("Picture")]
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.StockStatusBImage")]
        public int StockStatusBImage { get; set; }
        public bool StockStatusBImage_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status o
        /// </summary>
        [UIHint("Picture")]
        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.StockStatusOImage")]
        public int StockStatusOImage { get; set; }
        public bool StockStatusOImage_OverrideForStore { get; set; }

        #region License

        //License Variable
        public string RegistrationForm { get; set; }

        public string LicenseInformation { get; set; }

        public bool IsLicenseActive { get; set; }

        #endregion
    }
}
