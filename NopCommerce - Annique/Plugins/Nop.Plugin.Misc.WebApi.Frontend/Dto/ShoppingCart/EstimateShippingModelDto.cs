using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class EstimateShippingModelDto : ModelDto
    {
        public int RequestDelay { get; set; }

        public bool Enabled { get; set; }

        public int? CountryId { get; set; }

        public int? StateProvinceId { get; set; }

        public string ZipPostalCode { get; set; }

        public bool UseCity { get; set; }

        public string City { get; set; }

        public IList<SelectListItemDto> AvailableCountries { get; set; }

        public IList<SelectListItemDto> AvailableStates { get; set; }
    }
}
