using Annique.Plugins.Nop.Customization.Models.DiscountAllocation;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.ShoppingCart;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.DiscountAllocation
{
    public interface IDiscountAllocationModelFactory
    {
        /// <summary>
        /// Prepare the discount Info list model
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartModel">shopping cart Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Discount list model
        /// </returns>
        Task<DiscountInfoListModel> PrepareDiscountInfoListModelAsync(Customer customer, ShoppingCartModel shoppingCartModel);
    }
}
