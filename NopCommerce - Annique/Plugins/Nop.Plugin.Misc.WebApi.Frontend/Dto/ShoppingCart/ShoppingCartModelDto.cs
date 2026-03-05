using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class ShoppingCartModelDto : ModelDto
    {
        public bool OnePageCheckoutEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public IList<ShoppingCartItemModelDto> Items { get; set; }

        public IList<CheckoutAttributeModelDto> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }

        public string MinOrderSubtotalWarning { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        public bool TermsOfServiceOnOrderConfirmPage { get; set; }

        public bool TermsOfServicePopup { get; set; }

        public DiscountBoxModelDto DiscountBox { get; set; }

        public GiftCardBoxModelDto GiftCardBox { get; set; }

        public OrderReviewDataModelDto OrderReviewData { get; set; } 
        
        public bool HideCheckoutButton { get; set; }

        public bool ShowVendorName { get; set; }

        #region Nested Classes

        public partial class ShoppingCartItemModelDto : ModelWithIdDto
        {
            public string Sku { get; set; }

            public string VendorName { get; set; }

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

            public bool DisableRemoval { get; set; }

            public IList<string> Warnings { get; set; }
        }

        #endregion
    }
}
