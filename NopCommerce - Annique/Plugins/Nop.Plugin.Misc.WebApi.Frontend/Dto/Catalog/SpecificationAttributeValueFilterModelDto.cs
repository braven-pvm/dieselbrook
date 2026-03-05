using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog
{
    /// <summary>
    /// Represents a specification attribute value filter model
    /// </summary>
    public partial class SpecificationAttributeValueFilterModelDto : ModelWithIdDto
    {
        /// <summary>
        /// Gets or sets the specification attribute option name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option color (RGB)
        /// </summary>
        public string ColorSquaresRgb { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the value is selected
        /// </summary>
        public bool Selected { get; set; }
    }
}
