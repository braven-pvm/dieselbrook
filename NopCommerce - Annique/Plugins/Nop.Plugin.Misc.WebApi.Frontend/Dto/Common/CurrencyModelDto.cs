using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class CurrencyModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string CurrencySymbol { get; set; }
    }
}
