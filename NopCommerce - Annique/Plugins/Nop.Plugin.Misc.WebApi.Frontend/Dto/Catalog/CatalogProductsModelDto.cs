using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a catalog products model
    /// </summary>
    public partial class CatalogProductsModelDto : BasePageableModelDto
    {
        /// <summary>
        /// Get or set a value indicating whether use standart or AJAX products loading (applicable to 'paging', 'filtering', 'view modes') in catalog
        /// </summary>
        public bool UseAjaxLoading { get; set; }

        /// <summary>
        /// Gets or sets the warning message
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the message if there are no products to return
        /// </summary>
        public string NoResultMessage { get; set; }

        /// <summary>
        /// Gets or sets the price range filter model
        /// </summary>
        public PriceRangeFilterModelDto PriceRangeFilter { get; set; }

        /// <summary>
        /// Gets or sets the specification filter model
        /// </summary>
        public SpecificationFilterModelDto SpecificationFilter { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer filter model
        /// </summary>
        public ManufacturerFilterModelDto ManufacturerFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product sorting is allowed
        /// </summary>
        public bool AllowProductSorting { get; set; }

        /// <summary>
        /// Gets or sets available sort options
        /// </summary>
        public IList<SelectListItemDto> AvailableSortOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change view mode
        /// </summary>
        public bool AllowProductViewModeChanging { get; set; }

        /// <summary>
        /// Gets or sets available view mode options
        /// </summary>
        public IList<SelectListItemDto> AvailableViewModes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to select page size
        /// </summary>
        public bool AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets available page size options
        /// </summary>
        public IList<SelectListItemDto> PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets a order by
        /// </summary>
        public int? OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a product sorting
        /// </summary>
        public string ViewMode { get; set; }

        /// <summary>
        /// Gets or sets the products
        /// </summary>
        public IList<ProductOverviewModelDto> Products { get; set; }
    }
}
