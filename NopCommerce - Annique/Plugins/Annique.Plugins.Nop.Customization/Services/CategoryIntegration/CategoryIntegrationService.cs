using Annique.Plugins.Nop.Customization.Domain;
using Nop.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services
{
    /// <summary>
    /// Category integration service
    /// </summary>
    public class CategoryIntegrationService : ICategoryIntegrationService
    {
        #region Fields

        protected readonly IRepository<CategoryIntegration> _categoryIntegrationRepository;

        #endregion

        #region Ctor

        public CategoryIntegrationService(IRepository<CategoryIntegration> categoryIntegrationRepository)
        {
            _categoryIntegrationRepository = categoryIntegrationRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a category Integration
        /// </summary>
        /// <param name="categoryIntegration">Category Integration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertCategoryIntegrationAsync(CategoryIntegration categoryIntegration)
        {
            await _categoryIntegrationRepository.InsertAsync(categoryIntegration);
        }

        /// <summary>
        /// Deletes a Category Integration
        /// </summary>
        /// <param name="categoryIntegration">categoryIntegration</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteCategoryIntegrationAsync(CategoryIntegration categoryIntegration)
        {
            await _categoryIntegrationRepository.DeleteAsync(categoryIntegration);
        }

        /// <summary>
        /// Gets a Category Integration 
        /// </summary>
        /// <param name="categoryIntegrationId">category Integration identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category Integration
        /// </returns>
        public async Task<CategoryIntegration> GetCategoryIntegrationAsync(int categoryIntegrationId)
        {
            return await _categoryIntegrationRepository.GetByIdAsync(categoryIntegrationId);
        }

        /// <summary>
        /// Gets a category integrations by category identifier
        /// </summary>
        /// <param name="categoryId">The category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category Integrations
        /// </returns>
        public async Task<IList<CategoryIntegration>> GetAllCategoryIntegrationByCategoryIdAsync(int categoryId)
        {
            var query = from ci in _categoryIntegrationRepository.Table
                        where ci.CategoryId == categoryId
                        select ci;

            var categoryIntegrations = await query.ToListAsync();

            return categoryIntegrations;
        }

        #endregion
    }
}
