using System.Collections.Generic;
using System.Linq;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Tax
{
    public partial class GetTaxTotalResponse : BaseDto
    {
        public GetTaxTotalResponse(TaxTotalResult response)
        {
            TaxTotal = response.TaxTotal;
            TaxRates = response.TaxRates;
            Errors = response.Errors;
        }

        /// <summary>
        /// Tax total
        /// </summary>
        public decimal TaxTotal { get; set; }

        /// <summary>
        /// Tax rates
        /// </summary>
        public SortedDictionary<decimal, decimal> TaxRates { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        public IList<string> Errors { get; set; }

        /// <summary>
        /// A value indicating whether request has been completed successfully
        /// </summary>
        public bool Success => !Errors.Any();
    }
}
