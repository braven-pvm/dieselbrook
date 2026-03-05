using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class GiftCardDto : ModelWithIdDto
    {
        public string CouponCode { get; set; }

        public string Amount { get; set; }

        public string Remaining { get; set; }
    }
}