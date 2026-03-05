using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class GetManufacturerProductsResponse : BaseDto
    {
        public string TemplateViewPath { get; set; }

        public CatalogProductsModelDto CatalogProductsModel { get; set; }
    }
}
