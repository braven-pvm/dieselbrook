using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class UpdateOrderParametersDto : BaseDto
    {
        /// <summary>
        /// The updated order
        /// </summary>
        public OrderDto UpdatedOrder { get; protected set; }

        /// <summary>
        /// The updated order item
        /// </summary>
        public OrderItemDto UpdatedOrderItem { get; protected set; }

        /// <summary>
        /// The price of item with tax
        /// </summary>
        public decimal PriceInclTax { get; set; }

        /// <summary>
        /// The price of item without tax
        /// </summary>
        public decimal PriceExclTax { get; set; }

        /// <summary>
        /// The quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The amount of discount with tax
        /// </summary>
        public decimal DiscountAmountInclTax { get; set; }

        /// <summary>
        /// The amount of discount without tax
        /// </summary>
        public decimal DiscountAmountExclTax { get; set; }

        /// <summary>
        /// Subtotal of item with tax
        /// </summary>
        public decimal SubTotalInclTax { get; set; }

        /// <summary>
        /// Subtotal of item without tax
        /// </summary>
        public decimal SubTotalExclTax { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Applied discounts
        /// </summary>
        public List<DiscountDto> AppliedDiscounts { get; set; } 

        /// <summary>
        /// Pickup point
        /// </summary>
        public PickupPointDto PickupPoint { get; set; }
    }
}
