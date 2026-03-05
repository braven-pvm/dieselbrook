using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ProductSpecificationAttributeGroupModelDto : ModelWithIdDto
    {
        /// <summary>
        /// Gets or sets the specification attribute group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute group attributes
        /// </summary>
        public IList<ProductSpecificationAttributeModelDto> Attributes { get; set; }
    }
}
