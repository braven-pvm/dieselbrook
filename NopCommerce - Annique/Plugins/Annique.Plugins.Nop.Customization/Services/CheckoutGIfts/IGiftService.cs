using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.CheckoutGifts
{
    /// <summary>
    /// GiftService interface
    /// </summary>
    public interface IGiftService
    {
        /// <summary>
        /// Gets all Gifts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gifts
        /// </returns>
        Task<IList<Gift>> GetAllGiftsAsync();

        /// <summary>
        /// Gets all Force GiftType Gifts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Force gifts
        /// </returns>
        Task<IList<Gift>> GetAllForceGiftsAsync();

        /// <summary>
        /// Gets all Blank giftType Gifts
        /// </summary>
        /// <param name="orderTotal">Order Total</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Blank gift type gifts
        /// </returns>
        Task<IList<Gift>> GetAllBlankGiftsAsync(decimal orderTotal);

        /// <summary>
        /// Gets a gifts by product Id
        /// </summary>
        /// <param name="productId">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gifts 
        /// </returns>
        Task<List<Gift>> GetGiftsByProductIdsAsync(int[] productIds);

        /// <summary>
        /// Gets a gift by product Id
        /// </summary>
        /// <param name="productId">ProductId Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gift
        /// </returns>
        Task<Gift> GetGiftByProductIdAsync(int productId);

        /// <summary>
        /// Gets a Starter Gift
        /// </summary>
        /// <param name="orderTotal">Order Total</param>
        /// <returns>
        /// The task result contains the gift
        /// </returns>
        Task<Gift> GetStarterGiftByOrderTotalAsync(decimal orderTotal);

        /// <summary>
        /// Returns Wheather product is gift product or not
        /// </summary>
        /// <param name="productId">product Identifier</param>
        /// The task result returns true if product is gift product else return false
        //bool IsGiftProduct(int productId);

        /// <summary>
        /// Returns gift id and shopping cart item id
        /// </summary>
        /// <param name="customerId">customer Identifier</param>
        /// <param name="storeId">Store Identifier</param>
        /// The task result returns gift id and shopping cart item id
        Task<List<(int giftId, int sciId)>> GetExistGiftItemInCartAsync(int customerId, string giftType, IList<ShoppingCartItem> cart);
        
        /// <summary>
        /// Returns Wheather customer is eligible to get Starter gifts or not
        /// <param name="customerId">Customer Identifier</param>
        /// </summary>
        /// The task result returns true if customer is new or first sale duration is on for customer
        Task<bool> IsEligibleForStarterGiftAsync(int customerId);

        /// <summary>
        /// Add gift product to cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="productId">ProductId</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store Identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task AddGiftProductInShoppingCartAsync(IList<ShoppingCartItem> cart, int productId, int quantity, Customer customer, int storeId);

        /// <summary>
        /// Inserts a gift taken
        /// </summary>
        /// <param name="giftsTaken">Gifts Taken</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertGiftsTakenAsync(GiftsTaken giftsTaken);

        /// <summary>
        /// Get a total of qty of gift taken
        /// </summary>
        /// <param name="giftId">Gifts Id</param>
        /// <param name="customerId">Customer Id</param>
        Task<int> GetGiftTakenQtyTotalAsync(int giftId, int customerId);

        /// <summary>
        /// Returns Wheather gift taken record already exit for order item
        /// </summary>
        /// <param name="orderItemId">Order Item Identifier</param>
        /// The task result returns true if record with orderItem already exist else false
        Task<bool> IsGiftTakenAlreadyExistAsync(int orderItemId);

        /// <summary>
        /// Get productids for Non editable cart products for cart page
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <param name="storeId">Store Identifier</param>
        //Task<List<int>> GetNonEditableCartProductsAsync(int customerId, int storeId);
        Task<List<int>> GetNonEditableCartProductsAsync(Customer customer, Store store);

        /// <summary>
        /// Returns shopping cart total discounts
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="cartTotalBeforeDiscount">Shopping cart total before discount</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<string> GetShoppingCartTotalsDiscountAsync(IList<ShoppingCartItem> cart, decimal cartTotalBeforeDiscount);

        /// <summary>
        /// Returns Wheather gifts should be procesed or not
        /// <param name="cart">Shopping cart</param>
        /// </summary>
        /// The task result returns true if cart has other products besides events 
        Task<bool> CanProcessGiftsAsync(IList<ShoppingCartItem> cart, Customer customer);

        IList<ShoppingCartItem> FilterItemsWithoutGifts(IList<ShoppingCartItem> cart, List<Gift> gifts);
        IList<ShoppingCartItem> FilterItemsWithGifts(IList<ShoppingCartItem> cart, List<Gift> gifts);

        /// <summary>
        /// Process gifts on checkout 
        /// <param name="cart">Shopping cart</param>
        /// <param name="customer"/>
        /// </summary>
        /// The task process gifts on checkout
        Task ProcessGiftsAsync(IList<ShoppingCartItem> cart, Customer customer);

        /// <summary>
        /// Process gifts on order paid  
        /// <param name="orderItems">Order Items</param>
        /// <param name="customerId"/>Customer Id</param>
        /// </summary>
        /// The task process gifts taken on order paid
        Task ProcessGiftsTakenAsync(int customerId, IList<OrderItem> orderItems);
    }
}
