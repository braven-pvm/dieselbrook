using Annique.Plugins.Nop.Customization.Domain.ConsultantAwards;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ConsultantAwards
{
    public interface IAwardService
    {
        #region Award Methods

        /// <summary>
        /// Get awards by customer id
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the awards assigned to customer
        /// </returns>
        IList<Award> GetAwardsByCustomerId(int customerId);

        /// <summary>
        /// Gets a Award by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award
        /// </returns>
        Task<Award> GetAwardByIdAsync(int id);

        /// <summary>
        /// Update Award
        /// </summary>
        /// <param name="award">Award</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task UpdateAwardAsync(Award award);

        #endregion

        #region Award Shopping cart item methods

        /// <summary>
        /// Inserts a Award Shopping cart Item
        /// </summary>
        /// <param name="awardId">award Id</param>
        /// <param name="shoppingCartItem">Shopping Cart item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAwardShoppingCartItemAsync(int awardId, ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// updates a Award Shopping cart Item
        /// </summary>
        /// <param name="awardShoppingCartItem">award Shopping cart item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateAwardShoppingCartItemsAsync(AwardShoppingCartItem awardShoppingCartItem);

        /// <summary>
        /// Get list of Award Shopping cart Items
        /// </summary>
        /// <param name="customerId">CustomerId</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<AwardShoppingCartItem>> GetAwardShoppingCartItemsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get list of Award Shopping cart Items by AwardId
        /// </summary>
        /// <param name="awardId">award id</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<AwardShoppingCartItem>> GetAwardShoppingCartItemsByAwardIdAsync(int awardId);

        /// <summary>
        /// Gets a Award Shopping cart item by shopping cart item Id
        /// </summary>
        /// <param name="sciId">Shopping cart item Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award
        /// </returns>
        Task<AwardShoppingCartItem> GetAwardScibyShoppingCartItemIdAsync(int sciId);

        #endregion

        #region Award Attribute methods

        //Check for product attribute in attributeXml
        bool ContainsProductAttribute(string attributeXml);

        //check for Award product attribute in attributeXml
        bool ContainsAwardProductAttribute(string attributeXml);

        /// <summary>
        /// Gets selected awards 
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the selected Awards
        /// </returns>
        Task<IList<Award>> ParseAwardProductAttributeAsync(string attributesXml);

        /// <summary>
        /// Adds an Award attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="awardId">Award id</param>
        /// <param name="value">Value</param>
        /// <returns>Updated result (XML format)</returns>
        string AddAwardProductAttribute(string attributesXml, int awardId, string value);

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="attributesXml1">The attributes of the first product</param>
        /// <param name="attributesXml2">The attributes of the second product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <param name="ignoreQuantity">A value indicating whether we should ignore the quantity of attribute value entered by the customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<bool> AreProductAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes, bool ignoreQuantity = true);

        /// <summary>
        /// Get distinct award id from order items
        /// </summary>
        /// <param name="orderItems">Order items</param>
        /// <returns>
        /// The task result retuns distincts award ids
        IList<int> GetDistinctAwardProductAttributeIdsFromOrderItems(IList<OrderItem> orderItems);

        /// <summary>
        /// Process award takens on order paid
        /// </summary>
        /// <param name="orderItems">Order items</param>
        /// <returns>
        /// The task process awards taken on order paid
        Task ProcessAwardsTakenAsync(IList<OrderItem> orderItems);

        #endregion
    }
}
