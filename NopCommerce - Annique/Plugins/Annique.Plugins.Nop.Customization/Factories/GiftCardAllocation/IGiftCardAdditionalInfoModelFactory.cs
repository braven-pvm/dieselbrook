using Annique.Plugins.Nop.Customization.Models.GiftCardAllocation;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.GiftCardAllocation
{
    public interface IGiftCardAdditionalInfoModelFactory
    {
        /// <summary>
        /// Prepare the Giftcard info List Model
        /// </summary>
        /// <param name="giftCards">List of the available giftCards</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Giftcard info list model
        /// </returns>
        Task<GiftCardInfoListModel> PrepareGiftCardInfoListModelAsync(IList<GiftCard> giftCards);
    }
}
