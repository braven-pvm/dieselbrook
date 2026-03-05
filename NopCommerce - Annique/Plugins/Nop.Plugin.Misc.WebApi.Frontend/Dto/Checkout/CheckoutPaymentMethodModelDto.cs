using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class CheckoutPaymentMethodModelDto : ModelDto
    {
        public IList<PaymentMethodModelDto> PaymentMethods { get; set; }

        public bool DisplayRewardPoints { get; set; }
        public int RewardPointsBalance { get; set; }
        public int RewardPointsToUse { get; set; }
        public string RewardPointsToUseAmount { get; set; }
        public bool RewardPointsEnoughToPayForOrder { get; set; }
        public bool UseRewardPoints { get; set; }
    }
}
