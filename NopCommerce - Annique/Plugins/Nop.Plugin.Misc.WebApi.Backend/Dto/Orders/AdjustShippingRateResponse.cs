using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class AdjustShippingRateResponse : BaseDto
    {
        /// <summary>
        /// Adjusted shipping rate
        /// </summary>
        public decimal AdjustedShippingRate { get; set; }

        /// <summary>
        /// Applied discounts
        /// </summary>
        public List<DiscountDto> AppliedDiscounts { get; set; }
    }
}
