using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class GetShoppingCartTotalResponse : BaseDto
    {
        /// <summary>
        /// Shopping cart total (Null if shopping cart total couldn't be calculated now)
        /// </summary>
        public decimal? ShoppingCartTotal { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal RedeemedRewardPointsAmount { get; set; }

        /// <summary>
        /// Applied discounts
        /// </summary>
        public List<DiscountDto> AppliedDiscounts { get; set; }

        /// <summary>
        /// Applied gift cards
        /// </summary>
        public List<AppliedGiftCardResponseDto> AppliedGiftCards { get; set; }

        /// <summary>
        /// Reward points to redeem
        /// </summary>
        public int RedeemedRewardPoints { get; set; }

        /// <summary>
        /// Reward points amount in primary store currency to redeem
        /// </summary>
        public decimal DiscountAmount { get; set; }
    }
}
