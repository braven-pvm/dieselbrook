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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XcellenceIT.Plugin.ProductRibbons.Models
{
    public record ProductRibbonModel : BaseNopModel, ILocalizedModel<RibbonProductLocalizedModel>
    {
        public ProductRibbonModel()
        {
            AvailableStores = new List<SelectListItem>();
            StoreList = new List<int>();
            ProductPictureRibbon = new ProductPictureModel();
            Locales = new List<RibbonProductLocalizedModel>();
            productRibbonSearchModel = new ProductRibbonSearchModel();

            productMappingModel = new ProductMappingModel();
        }

        public ProductRibbonSearchModel productRibbonSearchModel { get; set; }

        #region Manage Product Ribbon 

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Id")]
        public int Id { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.RibbonName")]
        public string RibbonName { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.ApplyToAllProduct")]
        public bool ApplyToAllProduct { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.MarkAsNew")]
        public bool MarkAsNew { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.StartDateUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDateUtc { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.EndDateUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDateUtc { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.IsMoreRibbonDisplayAfterThis")]
        public bool IsMoreRibbonDisplayAfterThis { get; set; }

        [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.StoreIds")]
        public string StoreIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<int> StoreList { get; set; }

        public ProductPictureModel ProductPictureRibbon { get; set; }

        public IList<RibbonProductLocalizedModel> Locales { get; set; }

        public ProductMappingModel productMappingModel { get; set; }
        #endregion

        public record ProductPictureModel : BaseNopEntityModel
        {
            public int RibbonId { get; set; }

            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.RibbonText")]
            public string RibbonText { get; set; }

            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Enabled")]
            public bool Enabled { get; set; }

            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.Position")]
            public int Position { get; set; }

            [UIHint("Picture")]
            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.PictureId")]
            public int PictureId { get; set; }

            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.TextStyleCss")]
            public string TextStyleCss { get; set; }

            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.ImageStyleCss")]
            public string ImageStyleCss { get; set; }

            [NopResourceDisplayName("XcellenceIT.Plugin.ProductRibbons.Fields.ContainerStyleCss")]
            public string ContainerStyleCss { get; set; }
        }

        public record ProductMappingModel : BaseNopEntityModel
        {
            public int RibbonId { get; set; }

            public int ProductId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.Product")]
            public string ProductName { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
            public bool Published { get; set; }

        }

        public partial record AddProductMappingModel : BaseNopModel
        {
            public AddProductMappingModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public int SearchStoreId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public int SearchVendorId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public int RibbonId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }
    }
}
