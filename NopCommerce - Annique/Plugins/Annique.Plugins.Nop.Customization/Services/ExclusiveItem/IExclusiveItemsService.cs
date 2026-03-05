using Annique.Plugins.Nop.Customization.Domain;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderNote = Nop.Core.Domain.Orders.OrderNote;

namespace Annique.Plugins.Nop.Customization.Services.ExclusiveItem
{
    /// <summary>
    /// ExclusiveItemsService interface
    /// </summary>
    public interface IExclusiveItemsService
    {
        /// <summary>
        /// Returns Wheather customer can access exclusive category of not
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if customer is exist in exclusive table otherwise returns false
        bool CanAccessExclusiveCategory(int customerId);

        /// <summary>
        /// Search Exclusive products
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Exclusive products
        /// </returns>
        Task<IEnumerable<Product>> SearchExclusiveProductsAsync(int customerId);

        /// <summary>
        /// Search Force Add to Cart Exclusive products
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Forced Add to cart Exclusive products
        /// </returns>
        IList<ExclusiveItems> SearchForceAddToCartExclusiveItems(int customerId);

        /// <summary>
        /// Get Exclusive Item
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Exclusive Item 
        /// </returns>
        Task<ExclusiveItems> GetExclusiveItemAsync(int productId, int customerId);

        /// <summary>
        /// Get Allocated Exclusive Item
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains exclusive item
        /// </returns>
        Task<ExclusiveItems> GetAllocatedExclusiveItemAsync(int productId, int customerId);

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="customer">Customer</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product category mapping collection
        /// </returns>
        Task<IList<ProductCategory>> GetProductCategoriesByProductIdAsync(int productId, int storeId,
            Customer customer, bool showHidden = false);

        /// <summary>
        /// Exclusive Item allocated
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the true if item allocated else returns false
        /// </returns>
        Task<bool> IsExclusiveItemAllocatedAsync(int productId, int customerId);

        /// <summary>
        /// Update Exclusive Item
        /// </summary>
        /// <param name="exclusiveItems">Exclusive Item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateExclusiveItemAsync(ExclusiveItems exclusiveItems);

        /// <summary>
        /// Search Exclusive Items
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Exclusive Items
        /// </returns>
        Task<IList<ExclusiveItems>> SearchStarterKitExclusiveItemsAsync(int customerId);

        /// <summary>
        /// Returns Wheather Force product is exclusive product or not
        /// </summary>
        /// <param name="productId">product Identifier</param>
        /// <param name="customerId">customer Identifier</param>
        /// The task result returns true if product is Force exclusive product else return false
        Task<bool> IsForceExclusiveProductAsync(int productId, int customerId);

        /// <summary>
        /// Returns Wheather product is exclusive product or not
        /// </summary>
        /// <param name="productId">product Identifier</param>
        /// <param name="customerId">customer Identifier</param>
        /// The task result returns true if product is exclusive product else return false
        bool IsStarterExclusiveProduct(int productId, int customerId);

        /// <summary>
        /// Returns exclusive items 
        /// </summary>
        /// <param name="productIds">product Identifier</param>
        /// The task result returns exclusive items
        Task<IList<ExclusiveItems>> GetExclusiveItemsByProductIdsAsync(IEnumerable<int> productIds);

        /// <summary>
        /// Returns Wheather starter product exist in cart or not
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// The task result returns true if product is exist in cart else return false
        bool IsStarterKitExistInCart(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Get Exclusive Item by Id
        /// </summary>
        /// <param name="Id">Product identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Exclusive Item 
        /// </returns>
        Task<ExclusiveItems> GetExclusiveItemByIdAsync(int id);

        /// <summary>
        /// Restore ExclusiveItems On Order Cancellation
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderNotes">Order notes</param>
        /// </param>
        /// <returns>
        /// </returns>
        Task RestoreExclusiveItemsOnOrderCancellationAsync(Order order, IList<OrderNote> orderNotes);


        /// <summary>
        /// Handle exclusive items on order place event
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">Order items</param>
        /// </param>
        Task HandleExclusiveItemsOnOrderPlaceAsync(Order order, IEnumerable<OrderItem> orderItems);

        /// <summary>
        /// Handle exclusive items on product details page
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// </param>
        /// If product is exclusive , then checks for is it allocated to user or not 
        /// If not allocated to that user then return user to home page
        Task<IActionResult> HandleExclusiveItemsAsync(int productId);
    }
}
