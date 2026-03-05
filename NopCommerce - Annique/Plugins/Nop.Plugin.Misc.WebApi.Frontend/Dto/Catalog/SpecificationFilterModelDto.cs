using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a specification filter model
    /// </summary>
    public partial class SpecificationFilterModelDto : ModelDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the filtrable specification attributes
        /// </summary>
        public IList<SpecificationAttributeFilterModelDto> Attributes { get; set; }
    }
}
