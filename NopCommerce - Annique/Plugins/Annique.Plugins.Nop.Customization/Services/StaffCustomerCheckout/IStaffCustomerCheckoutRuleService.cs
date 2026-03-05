using DocumentFormat.OpenXml.Wordprocessing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Web.Models.ShoppingCart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.StaffCustomerCheckout
{
    /// <summary>
    /// StaffCustomerCheckoutRuleService interface
    /// </summary>
    public interface IStaffCustomerCheckoutRuleService
    {
        /// <summary>
        /// Get Customers's current calender month's total order count
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains Customers's current calender month's total order count
        /// </returns>
        Task<int> GetCustomerTotalOrderCountAsync(int customerId);

        /// <summary>
        /// Validate order total amount and exceed amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the true - OK; false - order total amount is valid or not, if not valid then how much amount exceed from order amount limit
        /// </returns>
        Task<(bool isValidAmount, decimal exceedAmount)> ValidateOrderTotalAmountAsync(IList<ShoppingCartItem> cart, Customer customer);

        /// <summary>
        /// process staff shopping cart validation
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="model">Shopping cart model</param>
        /// <param name="anniqueCustomizationSettings">Annique customization settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task<ShoppingCartModel> ProcessStaffShoppingCartValidationsAsync(Customer customer, Store store, ShoppingCartModel model, AnniqueCustomizationSettings anniqueCustomizationSettings);

    }
}
