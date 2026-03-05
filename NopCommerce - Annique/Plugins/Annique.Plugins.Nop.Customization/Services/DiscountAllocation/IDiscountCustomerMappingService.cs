using Annique.Plugins.Nop.Customization.Domain.DiscountAllocation;
using Annique.Plugins.Nop.Customization.Models.DiscountAllocation;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShoppingCartModel = Nop.Web.Models.ShoppingCart.ShoppingCartModel;

namespace Annique.Plugins.Nop.Customization.Services.DiscountAllocation
{
    public interface IDiscountCustomerMappingService
    {
        /// <summary>
        /// Get discount customer mapping by discount id
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// </param>
        /// <returns>
        /// The task result returns true if discount customer mapping exist
        /// </returns>
        Task<bool> IsExitDiscountCustomerMappingByDiscountIdAsync(int discountId);
          
        /// <summary>
        /// Returns Wheather discount customer mapping 
        /// </summary>
        /// <param name="discountId">Discount Identifier</param>
        ///<param name="customerId"> Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns discount customer mapping
        Task<DiscountCustomerMapping> GetDiscountCustomerMappingAsync(int discountId, int customerId);

        /// <summary>
        /// Returns all discount customer mappings
        /// </summary>
        ///<param name="customerId"> Customer Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns discount customer mappings
        Task<IList<DiscountCustomerMapping>> GetAllDiscountCustomerMappingsAsync(int customerId);

        /// <summary>
        /// Gets auto applied discounts
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the discounts
        /// </returns>
        Task<IList<string>> GetAppliedDiscountsAsync(Customer customer);

        /// <summary>
        /// Gets available discount names
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the discount names
        /// </returns>
        Task<IList<AvailableDiscountModel>> GetAvailableDiscountNamesAsync(IList<DiscountCustomerMapping> discountCustomerMappings);

        /// <summary>
        /// Update Discount customer mapping
        /// </summary>
        /// <param name="discountCustomerMapping">Discount customer mapping</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task UpdateDiscountCustomerMappingAsync(DiscountCustomerMapping discountCustomerMapping);

        /// <summary>
        /// Gets discount names
        /// </summary>
        /// <param name="customer">customer</param>
        /// <param name="shoppingCartModel">Shopping cart model</param>
        /// <param name="discountCustomerMappings">Discount customer mapping</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the discounts names
        /// </returns>
        Task<(IList<AvailableDiscountModel>, IList<string>, bool HasAutoApplied)> GetDiscountNamesAsync(Customer customer, ShoppingCartModel shoppingCartModel, IList<DiscountCustomerMapping> discountCustomerMappings);

        /// <summary>
        /// Insert Discount usage
        /// </summary>
        /// <param name="discountUsage">Discount Usage</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task InsertDiscountUsageAsync(DiscountUsage discountUsage);

        /// <summary>
        /// Get Discount customer mapping
        /// </summary>
        /// <param name="discountCustomerMappingId">Discount Customer mapping id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task<DiscountCustomerMapping> GetDiscountCustomerMappingByIdAsync(int discountCustomerMappingId);

        /// <summary>
        /// Get OrderItem DiscountDetailsAsync
        /// </summary>
        /// <param name="orderItemsWithProducts">Order items with product pair</param>
        ///<param name="discountCustomerMappings">Discount customer mappings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// returns orderItemId , discount amount , discount Id applied on order items
        /// </returns>
        Task<List<(int OrderItemId, decimal DiscountAmount, int discountId)>> GetOrderItemDiscountDetailsAsync(IEnumerable<(OrderItem OrderItem, Product Product)> orderItemsWithProducts, IList<DiscountCustomerMapping> discountCustomerMappings);

        /// <summary>
        /// Handle order discounts and discount usage entries
        /// </summary>
        /// <param name="order">Order</param>
        ///<param name="orderItems">Order items</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task HandleOrderDiscountsAsync(Order order, IEnumerable<OrderItem> orderItems);

        /// <summary>
        /// Restore special discount mappings On Order Cancellation
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderNotes">Order notes</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task RestoreSpecialDiscountOnOrderCancellationAsync(Order order, IList<OrderNote> orderNotes);

        Task HandleSpecialDiscountCodeApplicationAsync(Customer customer, string discountCouponCode, ShoppingCartModel model);

        Task<(bool applyAccess,Discount discount)> CanApplySpecialDiscountAsync(Customer customer, string discountCouponCode);
    }
}
