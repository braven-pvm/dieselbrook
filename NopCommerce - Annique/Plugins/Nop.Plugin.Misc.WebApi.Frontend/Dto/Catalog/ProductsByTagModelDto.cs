using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ProductsByTagModelDto : ModelWithIdDto
    {
        public string TagName { get; set; }

        public string TagSeName { get; set; }

        public CatalogProductsModelDto CatalogProductsModel { get; set; }
    }
}
