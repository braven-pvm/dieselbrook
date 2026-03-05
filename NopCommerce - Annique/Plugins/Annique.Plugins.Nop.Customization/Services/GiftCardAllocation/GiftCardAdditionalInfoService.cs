using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Services.PrivateMessagePopUp;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.GiftCardAllocation
{
    /// <summary>
    /// GiftCard AdditionalInfo service
    /// </summary>
    public class GiftCardAdditionalInfoService : IGiftCardAdditionalInfoService
    {
        #region Fields

        private readonly IRepository<GiftCardAdditionalInfo> _giftCardAdditionalInfoRepository;
        private readonly IRepository<GiftCard> _giftCardRepository;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly ICustomPrivateMessageService _customPrivateMessageService;

        #endregion

        #region Ctor

        public GiftCardAdditionalInfoService(IRepository<GiftCardAdditionalInfo> giftCardAdditionalInfoRepository,
            IRepository<GiftCard> giftCardRepository,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            ICustomPrivateMessageService customPrivateMessageService)
        {
            _giftCardAdditionalInfoRepository = giftCardAdditionalInfoRepository;
            _giftCardRepository = giftCardRepository;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _customerService = customerService;
            _customPrivateMessageService = customPrivateMessageService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert GiftCard Additional Info
        /// </summary>
        /// <param name="giftCardAdditionalInfo">GiftCard Additional info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertGiftCardAdditionalInfoAsync(GiftCardAdditionalInfo giftCardAdditionalInfo)
        { 
            await _giftCardAdditionalInfoRepository.InsertAsync(giftCardAdditionalInfo);
        }

        /// <summary>
        /// Update GiftCard Additional Info
        /// </summary>
        /// <param name="giftCardAdditionalInfo">GiftCard Additional info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateGiftCardAdditionalInfoAsync(GiftCardAdditionalInfo giftCardAdditionalInfo)
        {
            await _giftCardAdditionalInfoRepository.UpdateAsync(giftCardAdditionalInfo);
        }

        /// <summary>
        /// Get giftcard info by giftcard id
        /// </summary>
        /// <param name="giftcardId">Giftcard identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the Giftcard additional info
        /// </returns>
        public GiftCardAdditionalInfo GetGiftCardAdditionalInfoByGiftcardId(int giftcardId)
        {
            var info = from i in _giftCardAdditionalInfoRepository.Table
                       where i.GiftCardId == giftcardId
                       select i;

            return info.FirstOrDefault();
        }

        /// <summary>
        /// Get list of giftcards 
        /// </summary>
        /// <param name="username">username identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the giftcards 
        /// </returns>
        public async Task<IList<GiftCard>> GetAllocatedGiftCardsByUsernameAsync(string username)
        { 
            var query = from gca in _giftCardAdditionalInfoRepository.Table
                        join gc in _giftCardRepository.Table on gca.GiftCardId equals gc.Id
                        where gca.Username == username 
                        && gc.IsGiftCardActivated
                        select gc;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Returns Wheather customer has access to giftcard
        /// </summary>
        /// <param name="giftCardId">Giftcard Identifier</param>
        ///<param name="username"> Customer Username</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result returns true if customer has access
        public bool CanAccessGiftCard(int giftCardId, string username)
        {
            var query = from gca in _giftCardAdditionalInfoRepository.Table
                        join gc in _giftCardRepository.Table on gca.GiftCardId equals gc.Id
                        where gca.GiftCardId == giftCardId
                        && gca.Username == username
                        select gc;

            if(query.Any())
                return true;

            return false;
        }

        /// <summary>
        /// validate giftcard
        /// </summary>
        /// <param name="giftCardCouponCode">Giftcard coupon code</param>
        ///<param name="customer"> Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task result validate and handle giftcard
        public async Task<string> ValidateGiftCardAsync(string giftCardCouponCode, Customer customer)
        {
            // Trim and check if coupon code is null or empty
            giftCardCouponCode = giftCardCouponCode?.Trim();
            if (string.IsNullOrEmpty(giftCardCouponCode))
                return null;

            // Get gift card by coupon code and validate
            var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftCardCouponCode)).FirstOrDefault();
            var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
            if (!isGiftCardValid)
                return null;

            // Check if the gift card is allocated and accessible by the current user
            var isAllocatedGiftCard = GetGiftCardAdditionalInfoByGiftcardId(giftCard.Id);
            if (isAllocatedGiftCard == null)
                return null;

            var canAccessGiftCard = CanAccessGiftCard(giftCard.Id, customer.Username);
            if (!canAccessGiftCard)
            {
                // If user does not have access, remove the gift card and return a warning message
                await _customerService.RemoveGiftCardCouponCodeAsync(customer, giftCard.GiftCardCouponCode);
                return await _localizationService.GetResourceAsync("Public.GiftCards.GiftCardAdditionalInfo.Access.Validation.Message");
            }

            // If everything is valid, return null (no issues)
            return null;
        }

        /// <summary>
        /// process giftcard usage on checkout 
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// The task process giftcard usage on checkout
        public async Task ProcessGiftcardUsageOnCheckoutAsync(Order order) 
        {
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var giftcardAllocated = GetGiftCardAdditionalInfoByGiftcardId(gcuh.GiftCardId);
                if (giftcardAllocated != null)
                {
                    var giftCard = await _giftCardService.GetGiftCardByIdAsync(giftcardAllocated.GiftCardId);
                    giftCard.IsGiftCardActivated = false;
                    await _giftCardService.UpdateGiftCardAsync(giftCard);

                    //handle private message
                    await _customPrivateMessageService.HandlePrivateMessageAsync(giftcardId: giftcardAllocated.Id);
                }
            }
        }

        #endregion
    }
}
