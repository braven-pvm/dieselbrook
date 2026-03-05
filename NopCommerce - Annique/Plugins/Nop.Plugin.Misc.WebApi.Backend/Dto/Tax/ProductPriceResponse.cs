using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Tax
{
    public partial class ProductPriceResponse : BaseDto
    {
        public ProductPriceResponse((decimal price, decimal taxRate) response)
        {
            Price = response.price;
            TaxRate = response.taxRate;
        }

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Tax rate
        /// </summary>
        public decimal TaxRate { get; set; }
    }
}
