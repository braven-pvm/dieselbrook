using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class CheckoutAttributeChangeResponse : BaseDto
    {
        public OrderTotalsModelDto OrderTotalsModel { get; set; }

        public string FormattedAttributes { get; set; }

        public int[] EnabledAttributeIds { get; set; }

        public int[] DisabledAttributeIds { get; set; }
    }
}
