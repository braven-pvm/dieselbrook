using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using Annique.Plugins.Nop.Customization.Models.CheckoutGifts;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.SpecialOffers
{
    public interface ISpecialOffersService
    {
        /// <summary>
        /// Gets active special offers and associated discounts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer and associated discount
        /// </returns>
        Task<IList<(Offers, Discount)>> GetActiveSpecialOfferListAsync();

        /// <summary>
        /// Gets a offer by id
        /// </summary>
        /// <param name="offerId">offer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer 
        /// </returns>
        Task<Offers> GetOfferByIdAsync(int offerId);

        /// <summary>
        /// Gets a offers by ids
        /// </summary>
        /// <param name="offerIds">offer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer list 
        /// </returns>
        Task<IList<Offers>> GetOffersByIdsAsync(int[] offerIds);

        /// <summary>
        /// Gets a offer list
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the offer list 
        /// </returns>
        Task<IList<OfferList>> GetAllOfferListAsync();

        /// <summary>
        /// Returns all product ids by list type and offer id
        /// </summary>
        /// <param name="offerId">offer Id</param>
        /// <param name="listType">List type of product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns product ids based on list type and offer id 
        Task<IEnumerable<int>> GetProductIdsByOfferTypeAsync(int offerId, string listType);

        /// <summary>
        /// Returns total of offer Type F products
        /// </summary>
        /// <param name="cart">cart items</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns total 
        Task<decimal> EvaluateFProductTotalsAsync(IEnumerable<ShoppingCartItem> cart);

        /// <summary>
        /// Returns offer valid or not
        /// </summary>
        /// <param name="offer">Offer</param>
        /// <param name="productIds">Product ids</param>
        /// <param name="cart">Shopping cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if offer rule satisfied otherwise return false
        Task<bool> IsOfferValidForCartAsync(Offers offer, IEnumerable<int> productIds, IList<ShoppingCartItem> cart);

        /// <summary>
        /// check provided product belongs to provied offerType or not
        /// </summary>
        /// <param name="productId">Product Identifier</param>
        /// <param name="offerType">Offer type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if product belong to offer type else return false
        Task<bool> IsProductInOfferTypeAsync(int productId, string offerType);

        /// <summary>
        /// Get OfferId by productId
        /// </summary>
        /// <param name="productId">Product Identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns offer Id
        Task<int> GetOfferIdByProductAsync(int productId);

        /// <summary>
        /// handle G products when F product removed from cart
        /// </summary>
        /// <param name="sci">Shopping cart item</param>
        /// <param name="cart">Shopping cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task removes associated G products from cart when F type product removed from cart
        Task HandleGProductsOnFProductRemovalAsync(ShoppingCartItem sci, IList<ShoppingCartItem> cart);

        /// <summary>
        /// Gets selected attribute discount ids
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected attribute discountid</returns>
        IList<int> ParseAttributeDiscountIds(string attributesXml);

        /// <summary>
        /// Gets status for special offer
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Returns if has special offer and is it fully free or not</returns>
        SpecialOfferStatus ParseSpecialOfferStatus(string attributesXml); 

        /// <summary>
        /// Check for SpecialOffer product attribute in attributeXml
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Returns true if contains special attribute else return false</returns>
        bool ContainsSpecialOfferAttribute(string attributeXml);

        /// <summary>
        /// Is total rsp offer in attribute xml
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Returns true is total rsp offer exist in cart items attribute xml</returns>
        bool IsTotalRspOfferInAttributesXml(string attributesXml);

        /// <summary>
        /// Is matching Special attribute by offer Id
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="offerId">Offer id</param>
        /// <returns>Returns true for attribute match with offer id</returns>
        bool IsMatchingSpecialOfferAttribute(string attributesXml, int offerId);

        /// <summary>
        /// Adds an special offer attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="offerId">Offer id</param>
        /// <returns>Updated result (XML format)</returns>
        Task<string> AddSpecialOfferAttributeAsync(string attributesXml, int offerId, int discountId);

        //check standards warnings for G products before this product displayed in special offer product
        Task<bool> HasStandardWarningsAsync(Customer customer, Product product, int storeId, int quantity);

        //calculate special discount first, then calculate other standard discounts on discounted unit price
        Task<(decimal finalPrice, decimal totalDiscountAmount, List<Discount> appliedDiscounts)> ApplySpecialAndStandardDiscountsAsync(Product product, string attributesXml, decimal basePrice, List<Discount> standardDiscounts);

        /// <summary>
        /// Calculate special discounted price and discount amout 
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="unitPriceWithDiscount">Unit price with discount</param>
        /// <param name="unitPriceWithoutDiscount">Unit price without discount</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task returns special unit price and special discount amount
        public (decimal, decimal) CalculateDiscountedPrice(Discount discount, decimal unitPriceWithDiscount, decimal unitPriceWithoutDiscount);

        /// <summary>
        /// Calculate allowed selection quantity
        /// </summary>
        /// <param name="offers">Offers</param>
        /// <param name="productIds">Product id of offer Type F</param>
        /// <param name="cart">Cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task returns allowed selection quantities for offer items
        Task<int> CalculateAllowedSelectionsAsync(Offers offers, IEnumerable<int> productIds, IList<ShoppingCartItem> cart);

        /// <summary>
        /// Adjust allowed selection based on G items in cart
        /// </summary>
        /// <param name="offers">Offers</param>
        /// <param name="allowedSelections">Allowed selection</param>
        /// <param name="cart">Cart</param>
        /// The task returns allowed selection quantities with adjusting already in cart quantities 
        int AdjustAllowedSelectionsBasedOnCartGProducts(Offers offer, int allowedSelections, IList<ShoppingCartItem> cart);

        /// <summary>
        /// Validate and adjust G products on cart at checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task validate and handles G products on cart at checkout

        Task ValidateAndAdjustGProductQtyInCartAsync(IList<ShoppingCartItem> cart, Customer customer);

        /// <summary>
        /// Validate and adjust G products on cart at checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="offer">Special Offer</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task validate and handles G products on cart at checkout
        Task ValidateAndAdjustGProductQtyInCartAsync(IList<ShoppingCartItem> cart, Offers offer, Customer customer);

        /// <summary>
        /// save special offer inside discount usage table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">order items</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task will add entry inside ANQ_Discountusage table for special offer products
        Task SaveSpecialOfferDiscountUsageHistoryAsync(Order order, IList<OrderItem> orderItems);
    }
}
