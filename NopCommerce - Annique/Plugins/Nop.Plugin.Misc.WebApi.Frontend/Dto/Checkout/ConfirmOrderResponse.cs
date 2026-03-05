namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class ConfirmOrderResponse : CheckoutRedirectResponse
    {
        public CheckoutConfirmModelDto Model { get; set; }
    }
}
