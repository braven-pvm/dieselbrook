using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class EstimateShippingResultModelDto : ModelDto
    {
        public IList<ShippingOptionModelDto> ShippingOptions { get; set; }

        public bool Success { get; set; }

        public IList<string> Errors { get; set; }
    }
}
