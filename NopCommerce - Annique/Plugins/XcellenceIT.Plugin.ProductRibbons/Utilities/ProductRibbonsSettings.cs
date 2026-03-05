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
using Nop.Core.Configuration;

namespace XcellenceIT.Plugin.ProductRibbons.Utilities
{
    public class ProductRibbonsSettings : ISettings
    {
        #region Configure

        /// <summary>
        /// Gets or sets the value indicting whether this Plugin is enabled
        /// </summary>
        public bool XITRibbonEnabled { get; set; }

        /// <summary>
        /// Gets or Sets the value of CSS Selector for Product Ribbons
        /// </summary>
        public string CSSProductBoxSelector { get; set; }

        /// <summary>
        /// Gets or Sets the value of Display Content on product Details Page
        /// </summary>
        public string CSSProductPagePicturesParentContainerSelector { get; set; }
        /// <summary>
        /// Gets or sets a help document url
        /// </summary>
        public string HelpDocumentURL { get; set; }

        /// <summary>
        /// Gets or sets a Specification Attribute Id
        /// </summary>
        public int SpecificationAttributeId { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status R
        /// </summary>
        public int StockStatusRImage { get; set; }

        /// <summary>
        /// Gets or sets a Image url for stock status R 
        /// </summary>
        public string StockStatusRImageUrl { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status G
        /// </summary>
        public int StockStatusGImage { get; set; }

        /// <summary>
        /// Gets or sets a Image Url for stock status G
        /// </summary>
        public string StockStatusGImageUrl { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status B
        /// </summary>
        public int StockStatusBImage { get; set; }
        /// <summary>
        /// Gets or sets a Image Url for stock status B
        /// </summary>
        public string StockStatusBImageUrl { get; set; }

        /// <summary>
        /// Gets or sets a Image for stock status O
        /// </summary>
        public int StockStatusOImage { get; set; }

        /// <summary>
        /// Gets or sets a Image Url for stock status O
        /// </summary>
        public string StockStatusOImageUrl { get; set; }

        #endregion

        #region License

        /// <summary>
        /// Gets or sets a license key of this plugin
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// Gets or sets a license settings
        /// </summary>
        public string OtherLicenseSettings { get; set; }

        #endregion
    }
}
