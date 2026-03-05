using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Models;
using XcellenceIT.Plugin.ProductRibbons.Services;

namespace XcellenceIT.Plugin.ProductRibbons.Factories
{
    public class ProductRibbonFactory : IProductRibbonFactory
    {
        #region Fields

        private readonly IProductRibbonsService _productRibbonsService;
        private readonly IProductService _productService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public ProductRibbonFactory(IProductRibbonsService productRibbonsService, 
            IProductService productService, 
            IBaseAdminModelFactory baseAdminModelFactory,
            IUrlRecordService urlRecordService)
        {
            _productRibbonsService = productRibbonsService;
            _productService = productService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare Product Ribbon List model
        /// </summary>
        /// <param name="searchModel">Product Ribbon Search model</param>
        /// <param name="productRibbonRecord"> product Ribbon Record models</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Product Ribbon List   
        /// </returns>
        public virtual async Task<ProductRibbonProductListModel> PrepareRibbonProductListModelAsync(ProductRibbonSearchModel searchModel, ProductRibbonRecord productRibbonRecord)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productRibbonRecord == null)
                throw new ArgumentNullException(nameof(productRibbonRecord));

            //get product categories
            var productRibbons = await _productRibbonsService.GetProductRibbonMappingRibbonIdAsync(productRibbonRecord.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new ProductRibbonProductListModel().PrepareToGridAsync(searchModel, productRibbons, () =>
            {
                return productRibbons.SelectAwait(async x => new ProductRibbonModel.ProductMappingModel
                {
                    Id = x.Id,
                    RibbonId = x.RibbonId,
                    ProductId = x.ProductId,
                    ProductName = await _productService.GetProductByIdAsync(x.ProductId) == null ? "Product is Deleted" : (await _productService.GetProductByIdAsync(x.ProductId)).Name,
                    Published = (await _productService.GetProductByIdAsync(x.ProductId)).Published,
                });
            });

            return model;

        }

        /// <summary>
        /// Prepare Add Product To Category Search Model
        /// </summary>
        /// <param name="searchModel">Add Product To Category Search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Add Product To Category SearchModel  
        /// </returns>
        public virtual async Task<AddProductToCategorySearchModel> PrepareAddProductToRibbonSearchModelAsync(AddProductToCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare Add Product To Ribbon List Model
        /// </summary>
        /// <param name="searchModel">Add Product To Category Search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Add Ribbon Product List Model 
        /// </returns>
        public virtual async Task<AddRibbonProductListModel> PrepareAddProductToRibbonListModelAsync(AddProductToCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AddRibbonProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        public async Task<ProductRibbonRecord> PrepareAddEditProductRibbonModel(ProductRibbonModel model)
        {
            var productRibbon = new ProductRibbonRecord();

            if (model.Id > 0)
            {
                productRibbon = await _productRibbonsService.GetProductRibbonByIdAsync(model.Id);

                productRibbon.RibbonName = model.RibbonName;
                productRibbon.ApplyToAllProduct = model.ApplyToAllProduct;
                productRibbon.MarkAsNew = model.MarkAsNew;
                productRibbon.IsMoreRibbonDisplayAfterThis = model.IsMoreRibbonDisplayAfterThis;
                productRibbon.Enabled = model.Enabled;
                productRibbon.StartDateUtc = model.StartDateUtc;
                productRibbon.EndDateUtc = model.EndDateUtc;
                productRibbon.DisplayOrder = model.DisplayOrder;
                productRibbon.StoreIds = model.StoreList.Count == 0 ? "0" : string.Join(",", model.StoreList);
            }
            else
            {
                productRibbon = new ProductRibbonRecord
                {
                    RibbonName = model.RibbonName,
                    Enabled = model.Enabled,
                    StartDateUtc = model.StartDateUtc,
                    EndDateUtc = model.EndDateUtc,
                    DisplayOrder = model.DisplayOrder,
                    ApplyToAllProduct = model.ApplyToAllProduct,
                    MarkAsNew = model.MarkAsNew,
                    IsMoreRibbonDisplayAfterThis = model.IsMoreRibbonDisplayAfterThis,
                    StoreIds = model.StoreList.Count == 0 ? "0" : string.Join(",", model.StoreList),
                };
            }

            return productRibbon;
        }

        public Task<ProductPictureRibbon> PrepareProductPictureRibbonModel(ProductRibbonModel model, int productRibbonId)
        {
            var productPictureRibbon = new ProductPictureRibbon
            {
                RibbonText = model.ProductPictureRibbon.RibbonText,
                RibbonId = productRibbonId,
                Position = model.ProductPictureRibbon.Position,
                PictureId = model.ProductPictureRibbon.PictureId,
                Enabled = model.ProductPictureRibbon.Enabled,
                ContainerStyleCss = model.ProductPictureRibbon.ContainerStyleCss,
                ImageStyleCss = model.ProductPictureRibbon.ImageStyleCss,
                TextStyleCss = model.ProductPictureRibbon.TextStyleCss
            };

            return Task.FromResult(productPictureRibbon);
        }

        public async Task<ProductRibbonModel> PrepareEditViewModel(ProductRibbonRecord productRibbonRecord)
        {
            var model = new ProductRibbonModel();

            if (productRibbonRecord != null)
            {
                model.Id = productRibbonRecord.Id;
                model.RibbonName = productRibbonRecord.RibbonName;
                model.Enabled = productRibbonRecord.Enabled;
                model.StartDateUtc = productRibbonRecord.StartDateUtc;
                model.EndDateUtc = productRibbonRecord.EndDateUtc;
                model.DisplayOrder = productRibbonRecord.DisplayOrder;
                model.ApplyToAllProduct = productRibbonRecord.ApplyToAllProduct;
                model.MarkAsNew = productRibbonRecord.MarkAsNew;
                model.IsMoreRibbonDisplayAfterThis = productRibbonRecord.IsMoreRibbonDisplayAfterThis;
                model.StoreIds = productRibbonRecord.StoreIds;
            }
            var productPictureRibbon = await _productRibbonsService.GetProductPictureRibbonByIdAsync(productRibbonRecord.Id);
            if (productPictureRibbon != null)
            {
                model.ProductPictureRibbon.RibbonText = productPictureRibbon.RibbonText;
                model.ProductPictureRibbon.RibbonId = productPictureRibbon.Id;
                model.ProductPictureRibbon.Position = productPictureRibbon.Position;
                model.ProductPictureRibbon.PictureId = productPictureRibbon.PictureId;
                model.ProductPictureRibbon.Enabled = productPictureRibbon.Enabled;
                model.ProductPictureRibbon.ContainerStyleCss = productPictureRibbon.ContainerStyleCss;
                model.ProductPictureRibbon.ImageStyleCss = productPictureRibbon.ImageStyleCss;
                model.ProductPictureRibbon.TextStyleCss = productPictureRibbon.TextStyleCss;
            }
            return model;
        }

        public virtual Task<ProductRibbonSearchModel> PrepareRibbonProductSearchModel(ProductRibbonSearchModel searchModel, ProductPictureRibbon productPictureRibbon)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productPictureRibbon == null)
                throw new ArgumentNullException(nameof(productPictureRibbon));

            searchModel.RibbonId = productPictureRibbon.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        #endregion

    }
}
