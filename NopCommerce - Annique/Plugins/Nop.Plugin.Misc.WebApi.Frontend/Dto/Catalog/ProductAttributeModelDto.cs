using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class ProductAttributeModelDto : ModelWithIdDto
    {
        /// <summary>
        /// Gets or sets the value IDs of the attribute
        /// </summary>
        public IList<int> ValueIds { get; set; }
    }
}
