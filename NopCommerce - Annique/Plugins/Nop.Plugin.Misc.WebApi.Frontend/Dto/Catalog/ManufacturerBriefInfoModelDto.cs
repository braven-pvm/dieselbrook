using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ManufacturerBriefInfoModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public bool IsActive { get; set; }
    }
}
