using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class CategoryResponse : BaseDto
    {
        public string TemplateViewPath { get; set; }

        public CategoryModelDto CategoryModelDto { get; set; }
    }
}
