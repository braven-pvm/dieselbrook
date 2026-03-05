using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class GetTaxTotalTaxRateResponse : BaseDto
    {
        /// <summary>
        /// Tax total
        /// </summary>
        public decimal TaxTotal { get; set; }

        /// <summary>
        /// Tax rates
        /// </summary>
        public SortedDictionary<decimal, decimal> TaxRates { get; set; }
    }
}
