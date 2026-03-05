using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class AddToCartModelDto : ModelDto
    {
        public int ProductId { get; set; }

        /// <summary>
        /// qty
        /// </summary>
        public int EnteredQuantity { get; set; }

        public string MinimumQuantityNotification { get; set; }

        public List<SelectListItemDto> AllowedQuantities { get; set; }

        /// <summary>
        /// price entered by customers
        /// </summary>
        public bool CustomerEntersPrice { get; set; }

        public decimal CustomerEnteredPrice { get; set; }

        public string CustomerEnteredPriceRange { get; set; }

        public bool DisableBuyButton { get; set; }

        public bool DisableWishlistButton { get; set; }

        /// <summary>
        /// rental
        /// </summary>
        public bool IsRental { get; set; }

        /// <summary>
        /// pre-order
        /// </summary>
        public bool AvailableForPreOrder { get; set; }

        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

        public string PreOrderAvailabilityStartDateTimeUserTime { get; set; }

        /// <summary>
        /// updating existing shopping cart or wishlist item?
        /// </summary>
        public int UpdatedShoppingCartItemId { get; set; }

        public ShoppingCartType? UpdateShoppingCartItemType { get; set; }
    }
}
