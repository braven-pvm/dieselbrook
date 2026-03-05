using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts
{
    public partial class DiscountValidationResult : BaseDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether discount is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the errors that a customer should see when entering a coupon code (in case if "IsValid" is set to "false")
        /// </summary>
        public IList<string> Errors { get; set; }
    }
}
