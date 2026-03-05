using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class CheckoutAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        public string PriceAdjustment { get; set; }

        public bool IsPreSelected { get; set; }
    }
}
