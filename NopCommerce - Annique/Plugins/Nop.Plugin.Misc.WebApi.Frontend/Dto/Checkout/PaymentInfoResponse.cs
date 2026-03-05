using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class PaymentInfoResponse : BaseDto
    {
        public CheckoutConfirmModelDto CheckoutConfirmModel { get; set; }

        public CheckoutPaymentInfoModelDto CheckoutPaymentInfoModel { get; set; }
    }
}
