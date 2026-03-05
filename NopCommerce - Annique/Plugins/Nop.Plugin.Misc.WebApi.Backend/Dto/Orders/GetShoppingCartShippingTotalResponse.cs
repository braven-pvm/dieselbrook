using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class GetShoppingCartShippingTotalResponse : BaseDto
    {
        /// <summary>
        /// Shipping total
        /// </summary>
        public decimal? ShippingTotal { get; set; }

        /// <summary>
        /// Applied tax rate
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Applied discounts
        /// </summary>
        public List<DiscountDto> AppliedDiscounts { get; set; }
    }
}
