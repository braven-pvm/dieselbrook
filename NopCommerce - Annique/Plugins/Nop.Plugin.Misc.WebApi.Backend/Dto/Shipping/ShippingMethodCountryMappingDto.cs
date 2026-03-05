using Newtonsoft.Json;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping
{
    /// <summary>
    /// Represents a shipping method country mapping
    /// </summary>
    public partial class ShippingMethodCountryMappingDto : DtoWithId
    {
        /// <summary>
        /// Gets or sets the shipping method identifier
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int ShippingMethodId { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int CountryId { get; set; }
    }
}
