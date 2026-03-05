using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class TierPriceModelDto : ModelDto
    {
        public string Price { get; set; }
        public decimal PriceValue { get; set; }

        public int Quantity { get; set; }
    }
}
