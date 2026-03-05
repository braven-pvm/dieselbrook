using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class SearchRequest : BaseDto
    {
        public SearchModelDto Model { get; set; }

        public CatalogProductsCommandDto Command { get; set; }
    }
}
