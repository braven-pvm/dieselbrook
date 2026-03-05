using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class OrderReviewDataModelDto : ModelDto
    {
        public bool Display { get; set; }

        public AddressModelDto BillingAddress { get; set; }

        public bool IsShippable { get; set; }

        public AddressModelDto ShippingAddress { get; set; }

        public bool SelectedPickupInStore { get; set; }

        public AddressModelDto PickupAddress { get; set; }

        public string ShippingMethod { get; set; }

        public string PaymentMethod { get; set; }

        public Dictionary<string, object> CustomValues { get; set; }
    }
}
