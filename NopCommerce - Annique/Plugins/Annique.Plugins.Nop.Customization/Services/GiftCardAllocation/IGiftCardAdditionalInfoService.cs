using Annique.Plugins.Nop.Customization.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.GiftCardAllocation
{
    /// <summary>
    /// GiftCard AdditionalInfo interface
    /// </summary>
    public interface IGiftCardAdditionalInfoService
    {
        /// <summary>
        /// Insert GiftCard Additional Info
        /// </summary>
        /// <param name="giftCardAdditionalInfo">GiftCard Additional info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertGiftCardAdditionalInfoAsync(GiftCardAdditionalInfo giftCardAdditionalInfo);

        /// <summary>
        /// Update GiftCard Additional Info
        /// </summary>
        /// <param name="giftCardAdditionalInfo">GiftCard Additional info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateGiftCardAdditionalInfoAsync(GiftCardAdditionalInfo giftCardAdditionalInfo);

        /// <summary>
        /// Get giftcard info by giftcard id
        /// </summary>
        /// <param name="giftcardId">Giftcard identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Giftcard additional info
        /// </returns>
        GiftCardAdditionalInfo GetGiftCardAdditionalInfoByGiftcardId(int giftcardId);

        /// <summary>
        /// Get list of giftcards 
        /// </summary>
        /// <param name="username">username identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the giftcards 
        /// </returns>
        Task<IList<GiftCard>> GetAllocatedGiftCardsByUsernameAsync(string username);

        /// <summary>
        /// Returns Wheather customer has access to giftcard
        /// </summary>
        /// <param name="giftCardId">Giftcard Identifier</param>
        ///<param name="username"> Customer Username</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if customer has access
        bool CanAccessGiftCard(int giftCardId, string username);

        /// <summary>
        /// validate giftcard
        /// </summary>
        /// <param name="giftCardCouponCode">Giftcard coupon code</param>
        ///<param name="customer"> Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result validate and handle giftcard
        Task<string> ValidateGiftCardAsync(string giftCardCouponCode, Customer customer);

        /// <summary>
        /// process giftcard usage on checkout 
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task process giftcard usage on checkout
        Task ProcessGiftcardUsageOnCheckoutAsync(Order order);
    }
}
