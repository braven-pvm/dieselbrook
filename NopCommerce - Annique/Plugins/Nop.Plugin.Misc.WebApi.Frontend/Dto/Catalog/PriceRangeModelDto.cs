using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a price range model
    /// </summary>
    public partial class PriceRangeModelDto : ModelDto
    {
        /// <summary>
        /// Gets or sets the "from" price
        /// </summary>
        public decimal? From { get; set; }

        /// <summary>
        /// Gets or sets the "to" price
        /// </summary>
        public decimal? To { get; set; }
    }
}
