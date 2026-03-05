using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a products price range filter model
    /// </summary>
    public partial class PriceRangeFilterModelDto : ModelDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the selected price range
        /// </summary>
        public PriceRangeModelDto SelectedPriceRange { get; set; }

        /// <summary>
        /// Gets or sets the available price range
        /// </summary>
        public PriceRangeModelDto AvailablePriceRange { get; set; }
    }
}
