using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Checkout
{
    public partial class ShippingMethodModelDto : ModelDto
    {
        public string ShippingRateComputationMethodSystemName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Fee { get; set; }
        public decimal Rate { get; set; }
        public int DisplayOrder { get; set; }
        public bool Selected { get; set; }

        public ShippingOptionDto ShippingOption { get; set; }
    }
}
