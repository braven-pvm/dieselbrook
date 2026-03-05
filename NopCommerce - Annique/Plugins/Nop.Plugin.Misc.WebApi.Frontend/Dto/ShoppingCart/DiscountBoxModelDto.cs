using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class DiscountBoxModelDto : ModelDto
    {
        public List<DiscountInfoModelDto> AppliedDiscountsWithCodes { get; set; }

        public bool Display { get; set; }

        public List<string> Messages { get; set; }

        public bool IsApplied { get; set; }        
    }
}
