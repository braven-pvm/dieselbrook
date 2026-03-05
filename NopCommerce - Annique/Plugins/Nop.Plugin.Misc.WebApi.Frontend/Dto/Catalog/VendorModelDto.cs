using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class VendorModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public bool AllowCustomersToContactVendors { get; set; }

        public PictureModelDto PictureModel { get; set; }

        public CatalogProductsModelDto CatalogProductsModel { get; set; }
    }
}
