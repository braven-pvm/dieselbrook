using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models;
using Annique.Plugins.Nop.Customization.Services;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories
{
    /// <summary>
    /// Represents the Category Integration Model Factory
    /// </summary>
    public class CategoryIntegrationModelFactory : ICategoryIntegrationModelFactory
    {
        #region Fields

        private readonly ICategoryIntegrationService _categoryIntegrationService;

        #endregion

        #region Ctor

        public CategoryIntegrationModelFactory(ICategoryIntegrationService categoryIntegrationService)
        {
            _categoryIntegrationService = categoryIntegrationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare Custom Category Tab model
        /// </summary>
        /// <param name="categoryId">Category Indetifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Custom Category Tab model
        /// </returns>
        public CustomCategoryTabInfoModel PrepareCustomCategoryTabModelInfoAsync(int categoryId)
        {
            var model = new CustomCategoryTabInfoModel();
            
            model.CategoryIntegrationModel.CategoryId = categoryId;
            PrepareCategoryIntegrationSearchModel(model.CategoryIntegrationSearchModel, categoryId);

            return model;
        }

        /// <summary>
        /// Prepare Category Integration search model
        /// </summary>
        /// <param name="searchModel">Category Integration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration search model
        /// </returns>
        public virtual CategoryIntegrationSearchModel PrepareCategoryIntegrationSearchModel(CategoryIntegrationSearchModel searchModel,
            int categoryId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.CategoryId = categoryId;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare Category Integration list model
        /// </summary>
        /// <param name="searchModel">Category Integration search model</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration list model
        /// </returns>
        public virtual async Task<CategoryIntegrationListModel> PrepareCategoryIntegrationListModelAsync(CategoryIntegrationSearchModel searchModel, int categoryId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var categoryIntegrations = (await _categoryIntegrationService.GetAllCategoryIntegrationByCategoryIdAsync(categoryId)).ToPagedList(searchModel); ;

            //prepare grid model
            var model = new CategoryIntegrationListModel().PrepareToGrid(searchModel, categoryIntegrations, () =>
            {
                return categoryIntegrations.Select(categoryIntegration =>
                {
                    var categoryIntegrationModel = new CategoryIntegrationModel
                    {
                        Id = categoryIntegration.Id,
                        CategoryId = categoryId,
                        IntegrationField = categoryIntegration.IntegrationField,
                        IntegrationValue = categoryIntegration.IntegrationValue
                    };
                    return categoryIntegrationModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare Category Integration model
        /// </summary>
        /// <param name="model">Category Integration model</param>
        /// <param name="categoryIntegration">Category Integration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration model
        /// </returns>
        public virtual CategoryIntegrationModel PrepareCategoryIntegrationModel(CategoryIntegrationModel model, CategoryIntegration categoryIntegration)
        {
            if (categoryIntegration != null)
            {
                model ??= new CategoryIntegrationModel();
                model.Id = categoryIntegration.Id;
                model.CategoryId = categoryIntegration.CategoryId;
                model.IntegrationField = categoryIntegration.IntegrationField;
                model.IntegrationValue = categoryIntegration.IntegrationValue;
            }
            return model;
        }

        /// <summary>
        /// Prepare Category Integration table Fields
        /// </summary>
        /// <param name="model">Category Integration model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Category Integration table fields
        /// </returns>
        public CategoryIntegration PrepareCategoryIntegrationFields(CategoryIntegrationModel model)
        {
            if (model != null)
            {
                var categoryIntegration = new CategoryIntegration()
                {
                    Id = model.Id,
                    CategoryId = model.CategoryId,
                    IntegrationField = model.IntegrationField,
                    IntegrationValue = model.IntegrationValue
                };
                return categoryIntegration;
            }
            return null;
        }

        #endregion 
    }
}

