using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class CheckoutRedirectResponse : BaseDto
    {
        public string RedirectToMethod { get; set; }

        public int? Id { get; set; }
    }
}
