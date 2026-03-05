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
using System;

namespace XcellenceIT.Plugin.ProductRibbons.Domain
{
    /// <summary>
    /// ProductRibbonRecord
    /// </summary>
    public partial class ProductRibbonRecord : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string RibbonName { get; set; }

        /// <summary>
        /// Gets or sets Enable
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the DisplayOrder
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the Apply To All product
        /// </summary>
        public bool ApplyToAllProduct { get; set; }

        /// <summary>
        /// Gets or sets the MarkAsNew
        /// </summary>
        public bool MarkAsNew { get; set; }

        /// <summary>
        /// Gets or sets the StartDateUtc
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the EndDateUtc
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the StoreIds
        /// </summary>
        public string StoreIds { get; set; }

        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public bool IsMoreRibbonDisplayAfterThis { get; set; }
    }
}
