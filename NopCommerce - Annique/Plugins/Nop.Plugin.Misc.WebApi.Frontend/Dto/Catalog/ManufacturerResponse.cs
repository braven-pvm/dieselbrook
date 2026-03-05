using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ManufacturerResponse : BaseDto
    {
        public string TemplateViewPath { get; set; }

        public ManufacturerModelDto ManufacturerModel { get; set; }
    }
}
