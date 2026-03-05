using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class ShipmentDetailsModelDto : ModelWithIdDto
    {
        public string TrackingNumber { get; set; }

        public string TrackingNumberUrl { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? ReadyForPickupDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public IList<ShipmentStatusEventModelDto> ShipmentStatusEvents { get; set; }

        public bool ShowSku { get; set; }

        public IList<ShipmentItemModelDto> Items { get; set; }

        public OrderDetailsModelDto Order { get; set; }
    }
}
