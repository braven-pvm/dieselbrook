using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class ParseCheckoutAttributeValuesResponse : BaseDto
    {
        public CheckoutAttributeDto Attribute { get; set; }
        public IList<CheckoutAttributeValueDto> Values { get; set; }
    }
}
