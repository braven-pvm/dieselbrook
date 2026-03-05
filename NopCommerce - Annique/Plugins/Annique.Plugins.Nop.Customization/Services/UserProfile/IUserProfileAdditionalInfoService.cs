using Annique.Plugins.Nop.Customization.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Services.Messages;
using Nop.Web.Models.ShoppingCart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.UserProfile
{
    public interface IUserProfileAdditionalInfoService
    {
        /// <summary>
        /// Insert user profile Info
        /// </summary>
        /// <param name="userProfileAdditionalInfo">User Profile info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertUserAdditionalInfoAsync(UserProfileAdditionalInfo userProfileAdditionalInfo);

        /// <summary>
        /// Update user profile Info
        /// </summary>
        /// <param name="userProfileAdditionalInfo">User Profile info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateUserAdditionalInfoAsync(UserProfileAdditionalInfo userProfileAdditionalInfo);

        /// <summary>
        /// Get user profile info by customer id
        /// </summary>
        /// 
        /// <param name="customerId">Customer identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the User profile Info
        /// </returns>
        Task<UserProfileAdditionalInfo> GetUserProfileAdditionalInfoByCustomerIdAsync(int customerId);

        /// <summary>
        /// Encrypt text in RC4
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <param name="encryptionPrivateKey">Encryption private key</param>
        /// <returns>Encrypted text</returns>
        string EncryptTextRC4(string data, string encryptionPrivateKey);

        /// <summary>
        /// Decrypt text in RC4
        /// </summary>
        /// <param name="cipherText">Text to decrypt</param>
        /// <param name="encryptionPrivateKey">Encryption private key</param>
        /// <returns>Decrypted text</returns>
        string DecryptTextRC4(string data, string encryptionPrivateKey);

        /// <summary>
        /// Get additional info by IdNumber
        /// </summary>
        /// <param name="idNumber">IdNumber</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the User profile Addiitonal Info
        /// </returns>
        Task<UserProfileAdditionalInfo> GetUserProfileAdditionalInfoByIdNumberAsync(string idNumber);

        /// <summary>
        /// Validate User Profile IdNumber
        /// </summary>
        /// <param name="userProfileAdditionalInfo">UserProfileAdditionalInfo</param>
        /// <param name="newIdNumber">New IdNumber</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ValidateUserIdNumberAsync(UserProfileAdditionalInfo userProfileAdditionalInfo, string newIdNumber);

        /// <summary>
        /// Login passed user
        /// </summary>
        /// <param name="customer">User to login</param>
        /// <param name="returnUrl">URL to which the user will return after authentication</param>
        /// <param name="isPersist">Is remember me</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result of an authentication
        /// </returns>
        Task<IActionResult> SignInCustomerAsync(Customer customer, string returnUrl, bool isPersist = false);

        /// <summary>
        /// Insert customer changes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertCustomerChangesAsync(Customer customer,string tableName,int changeId,string fieldName,string oldValue,string newValue);

        /// <summary>
        /// Gets Lookups by ctype
        /// </summary>
        /// <param name="ctype">ctype</param>
        /// <param name="storeId">customer registered storeId</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Lookups
        /// </returns>
        Task<IList<Lookups>> GetLookupsByCtypeAsync(string ctype, int storeId);

        Task<IList<SelectListItem>> GetSelectListAsync(string ctype, int storeId);

        /// <summary>
        /// validate user profile 
        /// </summary>
        /// <param name="customer">customer</param>
        /// <param name="customerRoleIds">customer role ids</param>
        /// <param name="model">SHopping cart model</param>
        /// <param name="settings">Annique customization settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task validates user profiles and email validation
        /// </returns>
        Task<string> ValidateUserProfileAsync(Customer customer, int[] customerRoleIds, ShoppingCartModel model, AnniqueCustomizationSettings settings);

        /// <summary>
        /// validate email by emailable API
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task validates email by emailable api returs true if email is deliverable else return false
        /// </returns>
        Task<bool> VerifyEmailByApiAsync(string email);

        /// <summary>
        ///Return password recovery by sms is enable or disable
        /// </summary>
        Task<bool> IsPasswordResetViaSmsEnabledAsync();

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains password recovery sms sent or not
        /// </returns>
        Task<bool> SendCustomerPasswordRecoverySmsAsync(Customer customer, int languageId);

        /// <summary>
        /// Sends sms notification
        /// </summary>
        /// <param name="settings">Annique customization setting</param>
        /// <param name="customerId">Customer Id</param>
        /// <param name="message">Message</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result returns true if sms sent else return false
        /// </returns>
        Task<bool> SendSmsNotificationAsync(AnniqueCustomizationSettings settings, int customerId, string message);

        /// <summary>
        /// Add custom tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Customer</param>
        /// <param name="store">STore</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddCustomTokensAsync(IList<Token> tokens, Customer customer, Store store);

        /// <summary>
        /// Set customer default country
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task SetDefaultCountryAsync(Customer customer);

        /// <summary>
        /// Set customer activation date on first order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateActivationDateOnFirstOrderAsync(Order order);

        Task<bool> IsPhoneNumberTakenAsync(string phoneNumber);

        Task<bool> IsPhoneOrWhatsappNumberTakenByOtherAsync(string phoneNumber, int currentCustomerId);

        Task ValidatePhoneNumberAsync(string phoneNumber, int customerId, string errorResourceKey);

    }
}
