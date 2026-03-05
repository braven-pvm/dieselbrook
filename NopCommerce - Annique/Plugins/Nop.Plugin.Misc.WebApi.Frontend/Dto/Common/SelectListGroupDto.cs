using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class SelectListGroupDto : BaseDto
    {
        public bool Disabled { get; set; }
        
        public string Name { get; set; }
    }
}
