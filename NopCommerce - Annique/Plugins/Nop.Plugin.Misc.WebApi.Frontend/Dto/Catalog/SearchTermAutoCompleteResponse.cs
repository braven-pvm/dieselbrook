using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class SearchTermAutoCompleteResponse : BaseDto
    {
        public string Label { get; set; }

        public int ProductId { get; set; }

        public string Producturl { get; set; }

        public string Productpictureurl { get; set; }

        public bool Showlinktoresultsearch { get; set; }
    }
}
