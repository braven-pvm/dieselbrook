using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class ShipmentItemModelDto : ModelWithIdDto
    {
        public string Sku { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public string AttributeInfo { get; set; }

        public string RentalInfo { get; set; }

        public int QuantityOrdered { get; set; }

        public int QuantityShipped { get; set; }
    }
}
