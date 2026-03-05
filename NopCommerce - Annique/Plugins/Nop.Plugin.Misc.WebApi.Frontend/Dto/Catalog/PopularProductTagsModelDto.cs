using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class PopularProductTagsModelDto : ModelDto
    {
        public int TotalTags { get; set; }

        public List<ProductTagModelDto> Tags { get; set; }
    }
}
