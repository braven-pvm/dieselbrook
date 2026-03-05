using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class CurrencySelectorModelDto : ModelDto
    {
        public IList<CurrencyModelDto> AvailableCurrencies { get; set; }

        public int CurrentCurrencyId { get; set; }
    }
}
