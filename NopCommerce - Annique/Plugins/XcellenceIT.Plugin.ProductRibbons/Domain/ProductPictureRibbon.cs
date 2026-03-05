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

using Nop.Core;
using Nop.Core.Domain.Localization;

namespace XcellenceIT.Plugin.ProductRibbons.Domain
{
    /// <summary>
    /// ProductPictureRibbon record 
    /// </summary>
    public partial class ProductPictureRibbon : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the RibbonId
        /// </summary>
        public int RibbonId { get; set; }

        /// <summary>
        /// Gets or sets the PictureId
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the Enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the RibbonText
        /// </summary>
        public string RibbonText { get; set; }

        /// <summary>
        /// Gets or sets the Position
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the TextStyleCss
        /// </summary>
        public string TextStyleCss { get; set; }

        /// <summary>
        /// Gets or sets the ImageStyleCss
        /// </summary>
        public string ImageStyleCss { get; set; }

        /// <summary>
        /// Gets or sets the ContainerStyleCss 
        /// </summary>
        public string ContainerStyleCss { get; set; }
    }
}
