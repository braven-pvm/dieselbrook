using System.Threading.Tasks;
using Nop.Services.Payments;

namespace Nop.Plugin.Misc.WebApi.Frontend.Services;

public interface ICheckoutService
{
    /// <summary>
    /// Generate an order GUID
    /// </summary>
    /// <param name="processPaymentRequest">Process payment request</param>
    Task GenerateOrderGuidAsync(ProcessPaymentRequest processPaymentRequest);

    /// <summary>
    /// Save process payment request
    /// </summary>
    /// <param name="processPaymentRequest">Process payment request</param>
    Task SavePaymentInfoAsync(ProcessPaymentRequest processPaymentRequest);

    /// <summary>
    /// Get process payment request
    /// </summary>
    /// <returns>Process payment request</returns>
    Task<ProcessPaymentRequest> GetPaymentInfoAsync();

    /// <summary>
    /// Clear payment info
    /// </summary>
    Task ClearPaymentInfoAsync();
}