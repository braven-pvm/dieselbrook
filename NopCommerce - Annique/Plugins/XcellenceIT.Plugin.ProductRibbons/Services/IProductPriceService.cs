using Nop.Core.Domain.Catalog;
using System.Threading.Tasks;

namespace XcellenceIT.Plugin.ProductRibbons.Services
{
    public interface IProductPriceService
    {
        /// <summary>
        /// Get Old Price and New Price Difference in Percentage
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Old Price and New Price Difference in Percentage
        /// </returns>
        Task<decimal> GetOldPriceNewPriceDifferencePercentageAsync(Product product);

        /// <summary>
        /// Get Old Price and New Price Difference in Value
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Old Price and New Price Difference in Value
        /// </returns>
        Task<decimal> GetOldPriceNewPriceDifferenceValueAsync(Product product);

        /// <summary>
        /// Get Max Discount in Percentage
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Get Max Discount in Percentage
        /// </returns>
        Task<decimal> GetMaxDiscountPercentageAsync(Product product);

        /// <summary>
        /// Get Max Discount Value
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Get Max Discount in Value
        /// </returns>
        Task<decimal> GetMaxDiscountValueAsync(Product product);

        /// <summary>
        /// Get Product Quantity
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Product Quantity
        /// </returns>
        Task<int> GetProductQuantityAsync(Product product);

        /// <summary>
        /// Get Product Out Of Stock
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Product Out Of Stock
        /// </returns>
        Task<string> GetProductOutOfStockAsync(Product product);
    }
}
