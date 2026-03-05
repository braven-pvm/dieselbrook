using System;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Directory
{
    /// <summary>
    /// Represents an exchange rate
    /// </summary>
    public partial class ExchangeRateDto : BaseDto
    {
        /// <summary>
        /// The three letter ISO code for the Exchange Rate, e.g. USD
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The conversion rate of this currency from the base currency
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// When was this exchange rate updated from the data source (the data XML feed)
        /// </summary>
        public DateTime UpdatedOn { get; set; }
    }
}
