using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.WebApi.Framework
{
    /// <summary>
    /// Represents settings of the nopCommerce mobile application
    /// </summary>
    public class WebApiMobileSettings: ISettings
    {
        public List<string> AllowedSettings { get; set; } = new();

        /// <summary>
        /// Dictionary which contains the pair of picture and product identifiers.
        ///
        /// Key - the picture identifier
        /// Value - the product identifier
        /// </summary>
        public Dictionary<int, int> ProductsSliderImages { get; set; } = new();

        public int SliderPictureSize { get; set; }

        public int SliderPictureThumbSize { get; set; }
    }
}
