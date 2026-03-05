using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog
{
    public partial class FinalPriceResponse : BaseDto
    {
        public decimal PriceWithoutDiscounts { get; set; }

        public decimal FinalPrice { get; set; }

        public decimal AppliedDiscountAmount { get; set; }

        public IList<DiscountDto> AppliedDiscounts { get; set; }
    }
}
