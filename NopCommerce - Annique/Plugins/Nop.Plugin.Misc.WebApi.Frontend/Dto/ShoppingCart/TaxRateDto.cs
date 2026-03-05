using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class TaxRateDto : ModelDto
    {
        public string Rate { get; set; }

        public string Value { get; set; }
    }
}