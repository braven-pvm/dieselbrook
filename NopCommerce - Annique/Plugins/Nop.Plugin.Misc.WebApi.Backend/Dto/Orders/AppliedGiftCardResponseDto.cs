using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    /// <summary>
    /// Applied gift card
    /// </summary>
    public partial class AppliedGiftCardResponseDto : BaseDto
    {
        /// <summary>
        /// Gets or sets the used value
        /// </summary>
        public decimal AmountCanBeUsed { get; set; }

        /// <summary>
        /// Gets the gift card
        /// </summary>
        public GiftCardDto GiftCard { get; set; }
    }
}
