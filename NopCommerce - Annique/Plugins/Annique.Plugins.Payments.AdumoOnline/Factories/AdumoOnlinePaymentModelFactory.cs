using Annique.Plugins.Payments.AdumoOnline.Models;
using Annique.Plugins.Payments.AdumoOnline.Services;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using System.Threading.Tasks;

namespace Annique.Plugins.Payments.AdumoOnline.Factories
{
    /// <summary>
    /// Adumo online payment model factory
    /// </summary>
    public class AdumoOnlinePaymentModelFactory : IAdumoOnlinePaymentModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAdumoOnlinePaymentService _adumoOnlinePaymentService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public AdumoOnlinePaymentModelFactory(ISettingService settingService,
            IStoreContext storeContext,
            IAdumoOnlinePaymentService adumoOnlinePaymentService,
            IWorkContext workContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _adumoOnlinePaymentService = adumoOnlinePaymentService;
            _workContext = workContext;
        }

        #endregion

        #region Method

        /// <summary>
        /// Prepare the Payment info model
        /// </summary>
        /// <param name="order">Order</param>
        ///<returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Payment info model
        /// </returns>
        public async Task<PaymentInfoModel> PreparePaymentInfoModelAsync(Order order)
        {
            //load setting according store id
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AdumoOnlineSettings>(store.Id) ?? throw new NopException("Adumo Online module cannot be loaded as configuration setting is missing");

            if (string.IsNullOrEmpty(settings.FormPostUrl) ||
                string.IsNullOrEmpty(settings.MerchantId) ||
                string.IsNullOrEmpty(settings.ApplicationId) ||
                string.IsNullOrEmpty(settings.Secret))
            {
                throw new NopException("Adumo Online module cannot be loaded as configuration setting is missing");
            }

            //get store curent currency
            var currency = await _workContext.GetWorkingCurrencyAsync();

            //get current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            //merchant reference 
            var merchantReference = customer.Username + "_" + order.Id;

            //generate jwt token
            var token = _adumoOnlinePaymentService.GetNewJwtToken(order.OrderTotal, merchantReference, settings);

            var successUrl = $"{store.Url}PaymentAdumoOnline/SuccessReturn/{order.OrderGuid}";
            var failUrl = $"{store.Url}PaymentAdumoOnline/FailureReturn/{order.OrderGuid}";

            // Create payment model
            PaymentInfoModel model = new()
            {
                FormPostUrl = settings.FormPostUrl,
                Cuid = settings.MerchantId,
                Auid = settings.ApplicationId,
                Mref = merchantReference,
                Currency = currency.CurrencyCode,
                Amount = order.OrderTotal.ToString("0.00"),
                Token = token,
                SuccessCallBackUrl = successUrl,
                FailCallBackUrl = failUrl,
            };

            return model;
        }

        #endregion
    }
}
