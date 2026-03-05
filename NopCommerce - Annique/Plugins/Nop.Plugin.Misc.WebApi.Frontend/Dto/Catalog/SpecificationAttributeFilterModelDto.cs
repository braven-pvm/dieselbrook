using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a specification attribute filter model
    /// </summary>
    public partial class SpecificationAttributeFilterModelDto : ModelWithIdDto
    {
        /// <summary>
        /// Gets or sets the specification attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the values
        /// </summary>
        public IList<SpecificationAttributeValueFilterModelDto> Values { get; set; }
    }
}
