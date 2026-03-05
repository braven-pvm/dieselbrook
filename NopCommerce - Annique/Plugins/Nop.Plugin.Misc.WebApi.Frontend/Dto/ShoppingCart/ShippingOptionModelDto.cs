using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class ShippingOptionModelDto : ModelDto
    {
        public string Name { get; set; }

        public string ShippingRateComputationMethodSystemName { get; set; }

        public string Description { get; set; }

        public string Price { get; set; }

        public decimal Rate { get; set; }

        public string DeliveryDateFormat { get; set; }

        public int DisplayOrder { get; set; }

        public bool Selected { get; set; }
    }
}
