using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class ProductBreadcrumbModelDto : ModelDto
    {
        public bool Enabled { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public IList<CategorySimpleModelDto> CategoryBreadcrumb { get; set; }
    }
}
