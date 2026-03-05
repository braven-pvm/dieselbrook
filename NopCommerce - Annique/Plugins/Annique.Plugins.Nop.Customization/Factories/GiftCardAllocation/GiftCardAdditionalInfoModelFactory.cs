using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Models.GiftCardAllocation;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.GiftCardAllocation
{
    public class GiftCardAdditionalInfoModelFactory : IGiftCardAdditionalInfoModelFactory
    {
        #region Fields

        private readonly IGiftCardService _giftCardService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public GiftCardAdditionalInfoModelFactory(IGiftCardService giftCardService,
            IPriceFormatter priceFormatter)
        {
            _giftCardService = giftCardService;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Method

        /// <summary>
        /// Prepare the Giftcard info List Model
        /// </summary>
        /// <param name="giftCards">List of the available giftCards</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Giftcard info list model
        /// </returns>
        public async Task<GiftCardInfoListModel> PrepareGiftCardInfoListModelAsync(IList<GiftCard> giftCards)
        {
            var model = new GiftCardInfoListModel();
            foreach (var giftcard in giftCards)
            {
                var isGiftCardValid = await _giftCardService.IsGiftCardValidAsync(giftcard);
                if (isGiftCardValid && !string.IsNullOrWhiteSpace(giftcard.GiftCardCouponCode))
                {
                    var giftCardModel = new GiftCardInfoListModel.GiftCardInfoModel()
                    {
                        CouponCode = giftcard.GiftCardCouponCode,
                        Message = giftcard.Message,
                    };
                    var giftAmount = await _giftCardService.GetGiftCardRemainingAmountAsync(giftcard);
                    giftCardModel.Amount = await _priceFormatter.FormatPriceAsync(giftAmount, true, false);
                    model.GiftCardModels.Add(giftCardModel);
                }
            }
            return model;
        }

        #endregion
    }
}
