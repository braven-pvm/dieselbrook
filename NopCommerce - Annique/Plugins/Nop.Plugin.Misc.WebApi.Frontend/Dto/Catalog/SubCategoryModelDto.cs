using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class SubCategoryModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public string Description { get; set; }

        public PictureModelDto PictureModel { get; set; }
    }
}
