using Annique.Plugins.Nop.Customization.Models.PickUpCollection;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.PickUpCollection
{
    public interface IPickUpCollectionModelFactory
    {
        /// <summary>
        /// Prepare PostNetStoreDeliveryModel with Filter PickUp Store
        /// </summary>
        /// <param name="PostNetStoreDeliveryModel">model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the filter pickup store Collections
        /// </returns>
        Task<PostNetStoreDeliveryModel> PrepareFilterPickUpStoreModelAsync(PostNetStoreDeliveryModel model);
    }
}
