using Annique.Plugins.Nop.Customization.Domain;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OTP
{
    public interface IOtpService
    {
        /// <summary>
        /// Gets a OTP record by customer ID
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the OTP table record
        /// </returns>
        Task<Otp> GetCustomerOtpRecordAsync(int customerId);

        /// <summary>
        /// Verifies entered OTP with exisiting OTP
        /// </summary>
        /// <param name="otp">OTP</param>
        /// <returns>
        /// The task returns true if OTP mathches otherwise return false
        /// </returns>
        bool VerifyOTP(int customerId, string enteredOTP);

        /// <summary>
        /// Update OTP record
        /// </summary>
        /// <param name="otp">OTP</param>
        /// <returns>
        /// The task Updates OTP record in table
        /// </returns>
        Task UpdateOtpRecordAsync(Otp otp);

        /// <summary>
        /// Check Otp is verified or not
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>
        /// The task result returns true if OTP is verified otherwise returns false
        /// </returns>
        Task<bool> IsOtpVerifiedAsync(int customerId);

        /// <summary>
        /// Send OTP
        /// </summary>
        /// <param name="storeId">Store Id</param>
        /// <param name="customerId">Customer Id</param>
        /// <param name="sendVia">Send via</param>
        /// <returns>
        /// Send OTP to customer via sms or mail
        /// </returns>
        Task<(bool success, string message)> SendOTPAsync(int storeId, int customerId, string sendVia);
    }
}
