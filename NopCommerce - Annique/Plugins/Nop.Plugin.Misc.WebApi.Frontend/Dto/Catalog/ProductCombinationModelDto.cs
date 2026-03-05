using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ProductCombinationModelDto : ModelDto
    {
        /// <summary>
        /// Gets or sets the attributes
        /// </summary>
        public IList<ProductAttributeModelDto> Attributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to the combination have stock
        /// </summary>
        public bool InStock { get; set; }
    }
}
