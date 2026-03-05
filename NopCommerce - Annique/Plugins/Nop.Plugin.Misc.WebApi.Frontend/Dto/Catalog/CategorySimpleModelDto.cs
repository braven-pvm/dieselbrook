using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class CategorySimpleModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public List<CategorySimpleModelDto> SubCategories { get; set; }

        public bool HaveSubCategories { get; set; }

        public string Route { get; set; }

        public PictureModelDto PictureModel { get; set; }
    }
}
