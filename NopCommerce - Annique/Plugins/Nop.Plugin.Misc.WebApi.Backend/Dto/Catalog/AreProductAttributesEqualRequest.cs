using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog
{
    public partial class AreProductAttributesEqualRequest : BaseDto
    {
        /// <summary>
        /// The attributes of the first product
        /// </summary>
        public string AttributesXml1 { get; set; }

        /// <summary>
        /// The attributes of the second product
        /// </summary>
        public string AttributesXml2 { get; set; }
    }
}