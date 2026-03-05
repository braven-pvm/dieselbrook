using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class GetUnitPriceResponse : BaseDto
    {
        /// <summary>
        /// Unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Applied discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Applied discounts
        /// </summary>
        public List<DiscountDto> AppliedDiscounts { get; set; }
    }
}
