using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Wishlist
{
    public partial class WishlistModelDto : ModelDto
    {
        public Guid CustomerGuid { get; set; }

        public string CustomerFullname { get; set; }

        public bool EmailWishlistEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public bool DisplayAddToCart { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public IList<WishlistShoppingCartItemModelDto> Items { get; set; }

        public IList<string> Warnings { get; set; }

        #region Nested Classes

        public partial class WishlistShoppingCartItemModelDto : ModelWithIdDto
        {
            public string Sku { get; set; }

            public PictureModelDto Picture { get; set; }

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }
            public decimal UnitPriceValue { get; set; }

            public string SubTotal { get; set; }
            public decimal SubTotalValue { get; set; }

            public string Discount { get; set; }
            public decimal DiscountValue { get; set; }

            public int? MaximumDiscountedQty { get; set; }

            public int Quantity { get; set; }

            public List<SelectListItemDto> AllowedQuantities { get; set; }

            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public string RentalInfo { get; set; }

            public bool AllowItemEditing { get; set; }

            public IList<string> Warnings { get; set; }
        }

        #endregion
    }
}
