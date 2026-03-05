using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a manufacturer filter model
    /// </summary>
    public partial class ManufacturerFilterModelDto : ModelDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the filtrable manufacturers
        /// </summary>
        public IList<SelectListItemDto> Manufacturers { get; set; }
    }
}
