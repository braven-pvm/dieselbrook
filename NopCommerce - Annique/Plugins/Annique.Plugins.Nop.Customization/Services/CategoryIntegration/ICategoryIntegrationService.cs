using Annique.Plugins.Nop.Customization.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services
{
    /// <summary>
    /// Category integration interface
    /// </summary>
    public interface ICategoryIntegrationService
    {
        /// <summary>
        /// Inserts a category Integration
        /// </summary>
        /// <param name="categoryIntegration">Category Integration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertCategoryIntegrationAsync(CategoryIntegration categoryIntegration);

        /// <summary>
        /// Deletes a Category Integration
        /// </summary>
        /// <param name="categoryIntegration">categoryIntegration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteCategoryIntegrationAsync(CategoryIntegration categoryIntegration);

        /// <summary>
        /// Gets a Category Integration 
        /// </summary>
        /// <param name="categoryIntegrationId">category Integration identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category Integration
        /// </returns>
        Task<CategoryIntegration> GetCategoryIntegrationAsync(int categoryIntegrationId);

        /// <summary>
        /// Gets a category integrations by category identifier
        /// </summary>
        /// <param name="categoryId">The category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category Integrations
        /// </returns>
        Task<IList<CategoryIntegration>> GetAllCategoryIntegrationByCategoryIdAsync(int categoryId);
    }
}
