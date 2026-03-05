using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class GetShoppingCartSubTotalResponse : BaseDto
    {
        /// <summary>
        /// Applied discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Applied discounts
        /// </summary>
        public List<DiscountDto> AppliedDiscounts { get; set; }

        /// <summary>
        /// Sub total (without discount)
        /// </summary>
        public decimal SubTotalWithoutDiscount { get; set; }

        /// <summary>
        /// Sub total (with discount)
        /// </summary>
        public decimal SubTotalWithDiscount { get; set; }

        /// <summary>
        /// Tax rates (of order sub total)
        /// </summary>
        public SortedDictionary<decimal, decimal> TaxRates { get; set; }
    }
}
