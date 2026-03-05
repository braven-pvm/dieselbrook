using System.Collections.Generic;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a model to get the catalog products
    /// </summary>
    public partial class CatalogProductsCommandDto : BasePageableModelDto
    {
        /// <summary>
        /// Gets or sets the price ('min-max' format)
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option ids
        /// </summary>
        public List<int> SpecificationOptionIds { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer ids
        /// </summary>
        public List<int> ManufacturerIds { get; set; }

        /// <summary>
        /// Gets or sets a order by
        /// </summary>
        public int? OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a product sorting
        /// </summary>
        public string ViewMode { get; set; }
    }
}
