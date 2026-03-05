using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class CompareProductsModelDto : ModelWithIdDto
    {
        public IList<ProductOverviewModelDto> Products { get; set; }

        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        public bool IncludeFullDescriptionInCompareProducts { get; set; }
    }
}
