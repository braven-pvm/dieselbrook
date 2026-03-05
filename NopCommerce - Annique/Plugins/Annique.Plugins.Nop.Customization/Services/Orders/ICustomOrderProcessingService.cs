using Nop.Core.Domain.Orders;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.Orders
{
    public interface ICustomOrderProcessingService
    {
        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="order">The order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ReOrderAsync(Order order);
    }
}
