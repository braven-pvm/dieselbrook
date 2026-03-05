using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class MiniShoppingCartModelDto : ModelDto
    {
        public IList<MiniShoppingCartItemModelDto> Items { get; set; }

        public int TotalProducts { get; set; }

        public string SubTotal { get; set; }

        public decimal SubTotalValue { get; set; }

        public bool DisplayShoppingCartButton { get; set; }

        public bool DisplayCheckoutButton { get; set; }

        public bool CurrentCustomerIsGuest { get; set; }

        public bool AnonymousCheckoutAllowed { get; set; }

        public bool ShowProductImages { get; set; }

        #region

        public partial class MiniShoppingCartItemModelDto : ModelWithIdDto
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public int Quantity { get; set; }

            public string UnitPrice { get; set; }
            public decimal UnitPriceValue { get; set; }

            public string AttributeInfo { get; set; }

            public PictureModelDto Picture { get; set; }
        }

        #endregion
    }
}
