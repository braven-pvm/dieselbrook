using Annique.Plugins.Payments.AdumoOnline.Models;
using Nop.Core.Domain.Orders;
using System.Threading.Tasks;

namespace Annique.Plugins.Payments.AdumoOnline.Factories
{
    /// <summary>
    /// Adumo online payment model factory Interface
    /// </summary>
    public interface IAdumoOnlinePaymentModelFactory
    {
        /// <summary>
        /// Prepare the Payment info model
        /// </summary>
        /// <param name="order">Order</param>
        ///<returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Payment info model
        /// </returns>
        Task<PaymentInfoModel> PreparePaymentInfoModelAsync(Order order);
    }
}
