using Nop.Core.Domain.Tax;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public class TaxTypeSelectorModelDto : ModelDto
    {
        public TaxDisplayType CurrentTaxType { get; set; }
    }
}
