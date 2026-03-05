using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ProductOverviewModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string SeName { get; set; }

        public string Sku { get; set; }

        public ProductType ProductType { get; set; }

        public bool MarkAsNew { get; set; }

        public ProductOverviewProductPriceModelDto ProductPrice { get; set; }

        public IList<PictureModelDto> PictureModels { get; set; }

        public ProductSpecificationModelDto ProductSpecificationModel { get; set; }

        public ProductReviewOverviewModelDto ReviewOverviewModel { get; set; }

        #region Nested Classes

        public partial class ProductOverviewProductPriceModelDto : ModelDto
        {
            public string OldPrice { get; set; }

            public decimal? OldPriceValue { get; set; }

            public string Price { get; set; }

            public decimal? PriceValue { get; set; }

            /// <summary>
            /// PAngV base price (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }

            public decimal? BasePricePAngVValue { get; set; }

            public bool DisableBuyButton { get; set; }

            public bool DisableWishlistButton { get; set; }

            public bool DisableAddToCompareListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }

            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
        }

        #endregion
    }
}
