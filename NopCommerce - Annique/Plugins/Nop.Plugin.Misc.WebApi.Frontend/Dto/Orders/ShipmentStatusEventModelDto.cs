using System;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class ShipmentStatusEventModelDto : ModelDto
    {
        public string Status { get; set; }

        public string EventName { get; set; }

        public string Location { get; set; }

        public string Country { get; set; }

        public DateTime? Date { get; set; }
    }
}
