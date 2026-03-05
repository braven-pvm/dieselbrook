namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class NewBillingAddressResponse : CheckoutRedirectResponse
    {
        public CheckoutBillingAddressModelDto Model { get; set; }
    }
}
