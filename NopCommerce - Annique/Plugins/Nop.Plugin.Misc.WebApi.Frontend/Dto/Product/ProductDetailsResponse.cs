using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Product
{
    public partial class ProductDetailsResponse : BaseDto
    {
        /// <summary>
        /// The product template view path
        /// </summary>
        public string ProductTemplateViewPath { get; set; }

        /// <summary>
        /// The product details model
        /// </summary>
        public ProductDetailsModelDto ProductDetailsModel { get; set; }
    }
}
