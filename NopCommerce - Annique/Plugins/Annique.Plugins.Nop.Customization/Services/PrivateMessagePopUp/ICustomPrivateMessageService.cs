using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.PrivateMessagePopUp
{
    public interface ICustomPrivateMessageService
    {
        /// <summary>
        /// Removes private message related to giftcard, award and discounts
        /// </summary>
        /// <param name="giftcardId">giftcard idenifier</param>
        /// <param name="awardId">award  idenifier</param>
        /// <param name="discountId">Discount idenifier</param>
        /// </param>
        /// <returns>
        /// A task that Removes private message related to giftcard, award and discounts when redeemed by user
        /// </returns>
        Task HandlePrivateMessageAsync(int? giftcardId = null, int? awardId = null, int? discountId = null);
    }
}
