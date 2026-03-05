using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Common
{
    public partial class SelectListItemDto : BaseDto
    {
        public bool Disabled { get; set; }

        public SelectListGroupDto Group { get; set; }

        public bool Selected { get; set; }
        
        public string Text { get; set; }
        
        public string Value { get; set; }
    }
}
