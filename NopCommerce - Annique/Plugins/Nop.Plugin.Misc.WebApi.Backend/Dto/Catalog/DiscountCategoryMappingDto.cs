using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog
{
    /// <summary>
    /// Represents a discount-category mapping class
    /// </summary>
    public partial class DiscountCategoryMappingDto : DtoWithId
    {
        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public int DiscountId { get; set; }
    }
}
