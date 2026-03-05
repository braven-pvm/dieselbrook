using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    public partial class SearchModelDto : ModelDto
    {
        /// <summary>
        /// Query string
        /// </summary>
        public string Q { get; set; }

        /// <summary>
        /// Category ID
        /// </summary>
        public int Cid { get; set; }

        public bool Isc { get; set; }

        /// <summary>
        /// Manufacturer ID
        /// </summary>
        public int Mid { get; set; }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public int Vid { get; set; }

        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        public bool Sid { get; set; }

        /// <summary>
        /// A value indicating whether "advanced search" is enabled
        /// </summary>
        public bool Advs { get; set; }

        /// <summary>
        /// A value indicating whether "allow search by vendor" is enabled
        /// </summary>
        public bool Asv { get; set; }

        public CatalogProductsModelDto CatalogProductsModel { get; set; }

        public IList<SelectListItemDto> AvailableCategories { get; set; }

        public IList<SelectListItemDto> AvailableManufacturers { get; set; }

        public IList<SelectListItemDto> AvailableVendors { get; set; }
    }
}
