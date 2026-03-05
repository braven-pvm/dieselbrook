using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories
{
    /// <summary>
    /// Represents the Category Integration Model Factory interface
    /// </summary>
    public interface ICategoryIntegrationModelFactory
    {
        /// <summary>
        /// Prepare Custom Category Tab model
        /// </summary>
        /// <param name="categoryId">Category Indetifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Custom Category Tab model
        /// </returns>
        CustomCategoryTabInfoModel PrepareCustomCategoryTabModelInfoAsync(int categoryId);

        /// <summary>
        /// Prepare Category Integration search model
        /// </summary>
        /// <param name="searchModel">Category Integration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration search model
        /// </returns>
        CategoryIntegrationSearchModel PrepareCategoryIntegrationSearchModel(CategoryIntegrationSearchModel searchModel,
            int categoryId);

        /// <summary>
        /// Prepare Category Integration list model
        /// </summary>
        /// <param name="searchModel">Category Integration search model</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration list model
        /// </returns>
        Task<CategoryIntegrationListModel> PrepareCategoryIntegrationListModelAsync(CategoryIntegrationSearchModel searchModel, int categoryId);

        /// <summary>
        /// Prepare Category Integration model
        /// </summary>
        /// <param name="model">Category Integration model</param>
        /// <param name="categoryIntegration">Category Integration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration model
        /// </returns>
        CategoryIntegrationModel PrepareCategoryIntegrationModel(CategoryIntegrationModel model, CategoryIntegration categoryIntegration);

        /// <summary>
        /// Prepare Category Integration table Fields
        /// </summary>
        /// <param name="model">Category Integration model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration table fields
        /// </returns>
        CategoryIntegration PrepareCategoryIntegrationFields(CategoryIntegrationModel model);
    }
}

