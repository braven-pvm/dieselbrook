using Annique.Plugins.Nop.Customization.Models.ConsultantAwards;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ConsultantAwards
{
    public interface IAwardsModelFactory
    {
        /// <summary>
        /// Prepare the Award list model
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award list model
        /// </returns>
        Task<AwardListModel> PrepareAwardListModelAsync(int customerId);

        /// <summary>
        /// Prepare the AwardProductListModel 
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="selectedAwardId">Selected award id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of Award Product ListModel
        /// </returns>
        Task<IEnumerable<AwardListModel.AwardProductListModel>> PrepareAwardProductListModelsAsync(IEnumerable<Product> products, int selectedAwardId);

        /// <summary>
        /// Prepare the AwardProductQuantityModel 
        /// </summary>
        /// <param name="awardId">Award Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award list model
        /// </returns>
        Task<IList<AwardProductQuantityModel>> PrepareAwardProductQuantityModelAsync(int awardId);
    }
}