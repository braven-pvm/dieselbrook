using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog
{
    /// <summary>
    /// Represents a product-product tag mapping
    /// </summary>
    public partial class ProductProductTagMappingDto : DtoWithId
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product tag identifier
        /// </summary>
        public int ProductTagId { get; set; }
    }
}
