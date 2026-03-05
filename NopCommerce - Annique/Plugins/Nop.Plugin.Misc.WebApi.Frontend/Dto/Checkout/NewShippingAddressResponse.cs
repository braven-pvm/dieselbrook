namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class NewShippingAddressResponse : CheckoutRedirectResponse
    {
        public CheckoutShippingAddressModelDto Model { get; set; }
    }
}
