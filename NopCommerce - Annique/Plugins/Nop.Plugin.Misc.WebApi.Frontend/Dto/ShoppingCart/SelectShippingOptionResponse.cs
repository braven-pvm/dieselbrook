using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class SelectShippingOptionResponse : BaseDto
    {
        public bool Success { get; set; }

        public OrderTotalsModelDto Model { get; set; }

        public List<string> Errors { get; set; }
    }
}
