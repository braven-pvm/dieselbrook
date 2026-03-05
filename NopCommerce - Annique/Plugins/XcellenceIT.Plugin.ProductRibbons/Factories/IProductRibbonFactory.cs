using Nop.Web.Areas.Admin.Models.Catalog;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Models;

namespace XcellenceIT.Plugin.ProductRibbons.Factories
{
    public interface IProductRibbonFactory
    {
        /// <summary>
        /// Prepare Product Ribbon List model
        /// </summary>
        /// <param name="searchModel">Product Ribbon Search model</param>
        /// <param name="productRibbonRecord"> product Ribbon Record models</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Product Ribbon List   
        /// </returns>
        Task<ProductRibbonProductListModel> PrepareRibbonProductListModelAsync(ProductRibbonSearchModel searchModel, ProductRibbonRecord productRibbonRecord);

        /// <summary>
        /// Prepare Add Product To Category Search Model
        /// </summary>
        /// <param name="searchModel">Add Product To Category Search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Add Product To Category SearchModel  
        /// </returns>
        Task<AddProductToCategorySearchModel> PrepareAddProductToRibbonSearchModelAsync(AddProductToCategorySearchModel searchModel);

        /// <summary>
        /// Prepare Add Product To Ribbon List Model
        /// </summary>
        /// <param name="searchModel">Add Product To Category Search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Add Ribbon Product List Model 
        /// </returns>
        Task<AddRibbonProductListModel> PrepareAddProductToRibbonListModelAsync(AddProductToCategorySearchModel searchModel);

        Task<ProductRibbonRecord> PrepareAddEditProductRibbonModel(ProductRibbonModel model);

        Task<ProductPictureRibbon> PrepareProductPictureRibbonModel(ProductRibbonModel model, int productRibbonId);

        Task<ProductRibbonModel> PrepareEditViewModel(ProductRibbonRecord productRibbonRecord);

        Task<ProductRibbonSearchModel> PrepareRibbonProductSearchModel(ProductRibbonSearchModel searchModel, ProductPictureRibbon productPictureRibbon);
    }
}
