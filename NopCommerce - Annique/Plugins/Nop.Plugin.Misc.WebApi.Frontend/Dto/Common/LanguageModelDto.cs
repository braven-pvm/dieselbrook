using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class LanguageModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string FlagImageFileName { get; set; }
    }
}
