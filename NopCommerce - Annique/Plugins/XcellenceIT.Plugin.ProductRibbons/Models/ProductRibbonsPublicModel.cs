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

using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace XcellenceIT.Plugin.ProductRibbons.Models
{
    public record ProductRibbonsPublicModel
    {
        public ProductRibbonsPublicModel()
        {
            publicRibbonModel = new List<PublicRibbonModel>();
        }

        public IList<PublicRibbonModel> publicRibbonModel { get; set; }
    }
    public record PublicRibbonModel : BaseNopEntityModel
    {
        public int RibbonId { get; set; }
        public bool Enabled { get; set; }

        public string RibbonText { get; set; }

        public string Position { get; set; }

        public int ProductId { get; set; }

        public int PictureId { get; set; }

        public string TextStyleCss { get; set; }

        public string ImageStyleCss { get; set; }

        public string ContainerStyleCss { get; set; }

        public string PictureUrl { get; set; }

        public string ProductSeName { get; set; }
    }
}
