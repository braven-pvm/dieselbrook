using Nop.Core.Caching;
using Nop.Core.Domain.Payments;
using Nop.Core;
using Nop.Services.Payments;
using System.Threading.Tasks;
using System;

namespace Nop.Plugin.Misc.WebApi.Frontend.Services
{
    public partial class CheckoutService: ICheckoutService
    {
        #region Fields

        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreContext _storeContext;
        protected readonly IWorkContext _workContext;
        protected readonly PaymentSettings _paymentSettings;

        #endregion

        #region Ctor

        public CheckoutService(IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            PaymentSettings paymentSettings)
        {
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _paymentSettings = paymentSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare payment info cash key
        /// </summary>
        /// <returns>Cache key</returns>
        protected virtual async Task<CacheKey> PreparePaymentInfoCacheKeyAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var key = _staticCacheManager.PrepareKeyForDefaultCache(WebApiFrontendDefaults.PaymentInfoKeyCache,
                    await _storeContext.GetCurrentStoreAsync(),
                    customer.CustomerGuid);
            return key;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate an order GUID
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        public virtual async Task GenerateOrderGuidAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            //we should use the same GUID for multiple payment attempts
            //this way a payment gateway can prevent security issues such as credit card brute-force attacks
            //in order to avoid any possible limitations by payment gateway we reset GUID periodically
            var previousPaymentRequest = await GetPaymentInfoAsync();
            if (_paymentSettings.RegenerateOrderGuidInterval > 0 && previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Save process payment request
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        public virtual async Task SavePaymentInfoAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var key = await PreparePaymentInfoCacheKeyAsync();
            await _staticCacheManager.SetAsync(key, processPaymentRequest);
        }

        /// <summary>
        /// Get process payment request
        /// </summary>
        /// <returns>Process payment request</returns>
        public virtual async Task<ProcessPaymentRequest> GetPaymentInfoAsync()
        {
            var key = await PreparePaymentInfoCacheKeyAsync();
            return await _staticCacheManager.GetAsync(key, () => new ProcessPaymentRequest());
        }

        /// <summary>
        /// Clear payment info
        /// </summary>
        public virtual async Task ClearPaymentInfoAsync()
        {
            var key = await PreparePaymentInfoCacheKeyAsync();
            await _staticCacheManager.RemoveAsync(key);
        }

        #endregion
    }
}
