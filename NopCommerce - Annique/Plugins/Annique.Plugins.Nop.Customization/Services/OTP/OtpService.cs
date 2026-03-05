using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.OTP;
using Annique.Plugins.Nop.Customization.Services.ApiServices;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OTP
{
    public class OtpService : IOtpService
    {
        #region Fields

        private readonly IRepository<Otp> _otpRepository;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IApiService _apiService;

        #endregion

        #region Ctor

        public OtpService(IRepository<Otp> otpRepository,
            IStoreContext storeContext,
            ISettingService settingService,
            IApiService apiService)
        {
            _otpRepository = otpRepository;
            _storeContext = storeContext;
            _settingService = settingService;
            _apiService = apiService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a OTP record by customer ID
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the OTP table record
        /// </returns>
        public async Task<Otp> GetCustomerOtpRecordAsync(int customerId)
        {
            var query = await _otpRepository.Table
                .Where(otp => otp.CustomerID == customerId && otp.Expiry > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return query;
        }

        /// <summary>
        /// Verifies entered OTP with exisiting OTP
        /// </summary>
        /// <param name="otp">OTP</param>
        /// <returns>
        /// The task returns true if OTP mathches otherwise return false
        /// </returns>
        public bool VerifyOTP(int customerId, string enteredOTP)
        {
            if (string.IsNullOrEmpty(enteredOTP) || string.IsNullOrWhiteSpace(enteredOTP))
                // Handle null input
                return false;

            int enteredOTPValue;
            if (int.TryParse(enteredOTP, out enteredOTPValue))
            {
                bool isOTPValid = _otpRepository.Table
                    .Any(otp => otp.CustomerID == customerId &&
                                otp.Expiry > DateTime.UtcNow &&
                                otp.OTP == enteredOTPValue);

                return isOTPValid;
            }

            // Invalid entered OTP format
            return false;
        }

        /// <summary>
        /// Update OTP record
        /// </summary>
        /// <param name="otp">OTP</param>
        /// <returns>
        /// The task Updates OTP record in table
        /// </returns>
        public async Task UpdateOtpRecordAsync(Otp otp)
        { 
            await _otpRepository.UpdateAsync(otp);
        }

        /// <summary>
        /// Check Otp is verified or not
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>
        /// The task result returns true if OTP is verified otherwise returns false
        /// </returns>
        public async Task<bool> IsOtpVerifiedAsync(int customerId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);
            if(!settings.IsOTP)
                return true;

            bool isVerified = _otpRepository.Table
                            .Any(otp => otp.CustomerID == customerId &&
                            otp.Expiry > DateTime.UtcNow &&
                            otp.Iverified);

            return isVerified;
        }

        /// <summary>
        /// Send OTP
        /// </summary>
        /// <param name="storeId">Store Id</param>
        /// <param name="customerId">Customer Id</param>
        /// <param name="sendVia">Send via</param>
        /// <returns>
        /// Send OTP to customer via sms or mail
        /// </returns>
        public async Task<(bool success, string message)> SendOTPAsync(int storeId, int customerId, string sendVia)
        {
            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeId);

            //Get OTP api Url
            var hostUrl = new Uri(settings.OTPApiUrl);

            //apped parameters
            //string relativePath = "?id=" + customerId + "&sendvia=" + sendVia + "&staging=" + settings.IsStageEnvType;
            string relativePath = $"?id={customerId}&sendvia={sendVia}&staging={settings.IsStageEnvType}";
            string url = hostUrl + relativePath;

            //call to API
            var apiResponse = await _apiService.GetAPIResponseAsync(url);

            // Handle the API response
            if (apiResponse.Content == "true")
            {
                return (true, null);
            }
            else
            {
                var errorData = JsonConvert.DeserializeObject<OtpApiErrorResponseModel>(apiResponse.Content);
                return (false, errorData.message);
            }
        }

        #endregion
    }
}
