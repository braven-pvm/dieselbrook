namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class PaymentMethodResponse : CheckoutRedirectResponse
    {
        public CheckoutPaymentMethodModelDto Model { get; set; }
    }
}
