using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Rss;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class CatalogController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopUrlHelper _nopUrlHelper;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public CatalogController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            INopUrlHelper nopUrlHelper,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _nopUrlHelper = nopUrlHelper;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities
        
        private async Task<bool> CheckCategoryAvailabilityAsync(Category category)
        {
            if (category is null)
                return false;

            var isAvailable = !category.Deleted;

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(category) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }
        
        private async Task<bool> CheckManufacturerAvailabilityAsync(Manufacturer manufacturer)
        {
            var isAvailable = true;

            if (manufacturer == null || manufacturer.Deleted)
                return false;

            var notAvailable =
                //published?
                !manufacturer.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(manufacturer) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(manufacturer);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }
        
        private Task<bool> CheckVendorAvailabilityAsync(Vendor vendor)
        {
            var isAvailable = !(vendor == null || vendor.Deleted || !vendor.Active);

            return Task.FromResult(isAvailable);
        }

        private async Task PrepareImagesAsync(List<CategorySimpleModelDto> models)
        {
            var secured = _webHelper.IsCurrentConnectionSecured();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var categories =
                (await _categoryService.GetCategoriesByIdsAsync(models.Select(m => m.Id).ToArray())).ToDictionary(
                    p => p.Id, p => p);

            foreach (var categoryModel in models)
            {
                if (!categories.ContainsKey(categoryModel.Id))
                    continue;

                var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey,
                    categoryModel.Id, pictureSize, true, language, secured, store);

                var pictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(categories[categoryModel.Id].PictureId);
                    string fullSizeImageUrl, imageUrl;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl,
                        Title = string.Format(await _localizationService
                            .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), categoryModel.Name),
                        AlternateText = string.Format(await _localizationService
                            .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), categoryModel.Name)
                    };

                    return pictureModel;
                });

                categoryModel.PictureModel = pictureModel.ToDto<PictureModelDto>();
            }
        }

        private async Task PrepareCategoryImageAsync(CategoryModel model)
        {
            var secured = _webHelper.IsCurrentConnectionSecured();
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey,
                    category.Id, pictureSize, true, language, secured, store);

            var pictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);

                (var fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), category.Name),
                    AlternateText = string.Format(await _localizationService
                        .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), category.Name)
                };

                return pictureModel;
            });

            model.PictureModel = pictureModel;
        }

        #endregion

        #region Categories

        /// <summary>
        /// Get category
        /// </summary>
        [HttpPost("{categoryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategory([FromBody] CatalogProductsCommandDto command, int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return NotFound("The category is not available");

            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                store.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

            //model
            var model = await _catalogModelFactory.PrepareCategoryModelAsync(category, command.FromDto<CatalogProductsCommand>());

            //template
            var templateViewPath = await _catalogModelFactory.PrepareCategoryTemplateViewPathAsync(category.CategoryTemplateId);

            //get category picture
            await PrepareCategoryImageAsync(model);

            var response = new CategoryResponse
            {
                CategoryModelDto = model.ToDto<CategoryModelDto>(),
                TemplateViewPath = templateViewPath
            };
            return Ok(response);
        }

        /// <summary>
        /// Get the category products
        /// </summary>
        [HttpPost("{categoryId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetCategoryProductsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCategoryProducts([FromBody] CatalogProductsCommandDto command, int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return NotFound("The category is not available");

            var model = await _catalogModelFactory.PrepareCategoryProductsModelAsync(category, command.FromDto<CatalogProductsCommand>());

            var response = new GetCategoryProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return Ok(response);
        }

        /// <summary>
        /// Get catalog root (list of categories)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CategorySimpleModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCatalogRoot([FromQuery] bool loadImage=false)
        {
            var model = await _catalogModelFactory.PrepareRootCategoriesAsync();
            
            var modelDto = model.Select(c => c.ToDto<CategorySimpleModelDto>()).ToList();

            if (loadImage)
                await PrepareImagesAsync(modelDto);

            return Ok(modelDto);
        }

        /// <summary>
        /// Get catalog sub categories
        /// </summary>
        /// <param name="id">Category identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IList<CategorySimpleModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCatalogSubCategories(int id)
        {
            var model = await _catalogModelFactory.PrepareSubCategoriesAsync(id);
            var modelDto = model.Select(c => c.ToDto<CategorySimpleModelDto>()).ToList();

            return Ok(modelDto);
        }

        /// <summary>
        /// Get categories on Home page
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CategorySimpleModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> HomePageCategories()
        {
            var model = await _catalogModelFactory.PrepareHomepageCategoryModelsAsync();

            var modelDto = model.Select(c => c.ToDto<CategoryModelDto>()).ToList();

            return Ok(modelDto);
        }

        #endregion

        #region Manufacturers

        /// <summary>
        /// Get manufacturer
        /// </summary>
        [HttpPost("{manufacturerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ManufacturerResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturer([FromBody] CatalogProductsCommandDto command, int manufacturerId)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return NotFound("The manufacturer is not available");

            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                store.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewManufacturer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name), manufacturer);

            //model
            var model = await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer, command.FromDto<CatalogProductsCommand>());

            //template
            var templateViewPath = await _catalogModelFactory.PrepareManufacturerTemplateViewPathAsync(manufacturer.ManufacturerTemplateId);

            var response = new ManufacturerResponse
            {
                ManufacturerModel = model.ToDto<ManufacturerModelDto>(),
                TemplateViewPath = templateViewPath
            };
            return Ok(response);
        }

        /// <summary>
        /// Get manufacturer products
        /// </summary>
        [HttpPost("{manufacturerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetManufacturerProductsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturerProducts([FromBody] CatalogProductsCommandDto command, int manufacturerId)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return NotFound("The manufacturer is not available");

            var model = await _catalogModelFactory.PrepareManufacturerProductsModelAsync(manufacturer, command.FromDto<CatalogProductsCommand>());
            
            var response = new GetManufacturerProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return Ok(response);
        }

        /// <summary>
        /// Get all manufacturers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ManufacturerModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ManufacturerAll()
        {
            var model = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();
            var modelDto = model.Select(c => c.ToDto<ManufacturerModelDto>()).ToList();

            return Ok(modelDto);
        }

        #endregion

        #region Vendors

        /// <summary>
        /// Vendor
        /// </summary>
        [HttpPost("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(VendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetVendor([FromBody] CatalogProductsCommandDto command, int vendorId)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return NotFound("The vendor is not available");

            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                store.Id);

            //model
            var model = await _catalogModelFactory.PrepareVendorModelAsync(vendor, command.FromDto<CatalogProductsCommand>());

            return Ok(model.ToDto<VendorModelDto>());
        }

        /// <summary>
        /// Get vendor products
        /// </summary>
        [HttpPost("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetVendorProductsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetVendorProducts([FromBody] CatalogProductsCommandDto command, int vendorId)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return NotFound("The vendor is not available");

            var model = await _catalogModelFactory.PrepareVendorProductsModelAsync(vendor, command.FromDto<CatalogProductsCommand>());

            var response = new GetVendorProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };
            return Ok(response);
        }

        /// <summary>
        /// Get all vendors
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<VendorModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return Ok(new List<VendorModelDto>());

            var model = await _catalogModelFactory.PrepareVendorAllModelsAsync();
            var modelDto = model.Select(c => c.ToDto<VendorModelDto>()).ToList();

            return Ok(modelDto);
        }

        #endregion

        #region Product tags

        /// <summary>
        /// Get products by tag
        /// </summary>
        [HttpPost("{productTagId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductsByTagModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetProductsByTag([FromBody] CatalogProductsCommandDto command, int productTagId)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(productTagId);
            if (productTag == null)
                return NotFound($"ProductTag by id={productTagId} not found.");

            var model = await _catalogModelFactory.PrepareProductsByTagModelAsync(productTag, command.FromDto<CatalogProductsCommand>());

            return Ok(model.ToDto<ProductsByTagModelDto>());
        }

        /// <summary>
        /// Get tag products
        /// </summary>
        [HttpPost("{productTagId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GetTagProductsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetTagProducts([FromBody] CatalogProductsCommandDto command, int productTagId)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(productTagId);
            if (productTag == null)
                return NotFound($"ProductTag by id={productTagId} not found.");

            var model = await _catalogModelFactory.PrepareTagProductsModelAsync(productTag, command.FromDto<CatalogProductsCommand>());
            var response = new GetTagProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return Ok(response);
        }

        /// <summary>
        /// Get all popular product tags
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PopularProductTagsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ProductTagsAll()
        {
            var model = await _catalogModelFactory.PreparePopularProductTagsModelAsync();
            
            return Ok(model.ToDto<PopularProductTagsModelDto>());
        }

        #endregion

        #region New (recently added) products page

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NewProductsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewProducts(CatalogProductsCommandDto command)
        {
            if (!_catalogSettings.NewProductsEnabled)
                return NotFound();

            var model = new NewProductsModel
            {
                CatalogProductsModel = await _catalogModelFactory.PrepareNewProductsModelAsync(command.FromDto<CatalogProductsCommand>())
            };

            return Ok(model.ToDto<NewProductsModelDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewProductsRss()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(store, x => x.Name)}: New products",
                "Information about products",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return BadRequest($"The setting {nameof(_catalogSettings.NewProductsEnabled)} is not true.");

            var items = new List<RssItem>();

            var storeId = store.Id;
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId: storeId);

            foreach (var product in products)
            {
                var seName = await _urlRecordService.GetSeNameAsync(product);
                var productUrl = await _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = seName }, _webHelper.GetCurrentRequestProtocol());
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
                var productDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), $"urn:store:{store.Id}:newProducts:product:{product.Id}", product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Length)));
                //}

            }
            feed.Items = items;
            return Ok(feed.GetContent());
        }

        #endregion

        #region Searching

        /// <summary>
        /// Search
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SearchModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Search(SearchRequest request)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                store.Id);

            if (request.Model == null)
                request.Model = new SearchModel().ToDto<SearchModelDto>();

            var response = await _catalogModelFactory.PrepareSearchModelAsync(request.Model.FromDto<SearchModel>(), request.Command.FromDto<CatalogProductsCommand>());

            return Ok(response.ToDto<SearchModelDto>());
        }

        /// <summary>
        /// Search term auto complete
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<SearchTermAutoCompleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SearchTermAutoComplete([FromQuery][Required] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(string.Empty);

            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return NotFound($"Term length is less {_catalogSettings.ProductSearchTermMinimumLength}.");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = await _productService.SearchProductsAsync(0,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                keywords: term,
                languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();
            var result = (from p in models
                          select new SearchTermAutoCompleteResponse
                          {
                              Label = p.Name,
                              ProductId = p.Id,
                              Producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              Productpictureurl = p.PictureModels.FirstOrDefault()?.ImageUrl,
                              Showlinktoresultsearch = showLinkToResultSearch
                          })
                .ToList();
            return Ok(result);
        }

        /// <summary>
        /// Search products
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SearchProductsResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SearchProducts([FromBody] SearchRequest request)
        {
            if (request.Model == null)
                request.Model = new SearchModel().ToDto<SearchModelDto>();

            var httpContext = _httpContextAccessor.HttpContext;
                if(httpContext != null && string.IsNullOrEmpty(httpContext.Request.QueryString.Value))
                    httpContext.Request.QueryString = new QueryString("?q=");

            var model = await _catalogModelFactory.PrepareSearchProductsModelAsync(request.Model.FromDto<SearchModel>(), request.Command.FromDto<CatalogProductsCommand>());
            var response = new SearchProductsResponse
            {
                CatalogProductsModel = model.ToDto<CatalogProductsModelDto>(),
                TemplateViewPath = "_ProductsInGridOrLines"
            };

            return Ok(response);
        }

        #endregion
    }
}
