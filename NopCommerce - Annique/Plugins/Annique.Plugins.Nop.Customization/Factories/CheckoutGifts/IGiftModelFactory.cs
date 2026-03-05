using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Domain.SpecialOffers;
using Annique.Plugins.Nop.Customization.Models.CheckoutGifts;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.CheckoutGifts
{
    public interface IGiftModelFactory
    {
		/// <summary>
		/// Prepare the gifts model
		/// </summary>
		/// <param name="gifts">Blank Gifts</param>
		/// <param name="exclusiveItems">exclusive Item Model</param>
		/// <param name="activeOffers">Active offers</param>
		/// <returns>
		/// A task that represents the asynchronous operation
		/// The task result contains the Gift model
		/// </returns>
		Task<GiftModel> PrepareBlankGiftModelAsync(IList<Gift> gifts, IList<ExclusiveItems> exclusiveItems, IList<(Offers, Discount)> activeOffers, IList<ShoppingCartItem> cart);

        /// <summary>
        /// Prepare the special offer
        /// </summary>
        /// <param name="activeOffers">Active offers</param>
        /// <param name="cart">cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the special model
        /// </returns>
        Task<IList<GiftModel.SpecialOfferModel>> PrepareSpecialOfferModelAsync(IList<(Offers, Discount)> activeOffers,IList<ShoppingCartItem> cart);

        /// <summary>
        /// Prepare the special offer product model
        /// </summary>
        /// <param name="offerId">Offer Id</param>
        /// <param name="DiscountId">Discount Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the special product model
        /// </returns>
        Task<SpecialProductListModel> PrepareSpecialProductListModelAsync(int offerId, int discountId);
    }
}
