using Annique.Plugins.Nop.Customization.Domain.Enums;
using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Controllers
{
    public class AnniqueCatalogController : BasePublicController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly IAnniqueCustomizationConfigurationService _anniqueCustomizationConfigurationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISearchTermService _searchTermService;

        #endregion

        #region Ctor

        public AnniqueCatalogController(CatalogSettings catalogSettings,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreContext storeContext,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            IAnniqueCustomizationConfigurationService anniqueCustomizationConfigurationService,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            ICatalogModelFactory catalogModelFactory,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IVendorService vendorService,
            VendorSettings vendorSettings,
            IHttpContextAccessor httpContextAccessor,
            ISearchTermService searchTermService)
        {
            _catalogSettings = catalogSettings;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeContext = storeContext;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _anniqueCustomizationConfigurationService = anniqueCustomizationConfigurationService;
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
            _catalogModelFactory = catalogModelFactory;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _vendorSettings = vendorSettings;
            _httpContextAccessor = httpContextAccessor;
            _searchTermService = searchTermService;
        }

        #endregion

        #region utility

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        public virtual async Task<SearchModel> PrepareSearchModelAsync(SearchModel model, CatalogProductsCommand command, SearchOption? searchOption)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var categoriesModels = new List<SearchModel.CategoryModel>();
            //all categories
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: currentStore.Id);
            foreach (var c in allCategories)
            {
                //generate full category name (breadcrumb)
                var categoryBreadcrumb = string.Empty;
                var breadcrumb = await _categoryService.GetCategoryBreadCrumbAsync(c, allCategories);
                for (var i = 0; i <= breadcrumb.Count - 1; i++)
                {
                    categoryBreadcrumb += await _localizationService.GetLocalizedAsync(breadcrumb[i], x => x.Name);
                    if (i != breadcrumb.Count - 1)
                        categoryBreadcrumb += " >> ";
                }

                categoriesModels.Add(new SearchModel.CategoryModel
                {
                    Id = c.Id,
                    Breadcrumb = categoryBreadcrumb
                });
            }

            if (categoriesModels.Any())
            {
                //first empty entry
                model.AvailableCategories.Add(new SelectListItem
                {
                    Value = "0",
                    Text = await _localizationService.GetResourceAsync("Common.All")
                });
                //all other categories
                foreach (var c in categoriesModels)
                {
                    model.AvailableCategories.Add(new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Breadcrumb,
                        Selected = model.cid == c.Id
                    });
                }
            }

            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: currentStore.Id);
            if (manufacturers.Any())
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Value = "0",
                    Text = await _localizationService.GetResourceAsync("Common.All")
                });
                foreach (var m in manufacturers)
                    model.AvailableManufacturers.Add(new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = await _localizationService.GetLocalizedAsync(m, x => x.Name),
                        Selected = model.mid == m.Id
                    });
            }

            model.asv = _vendorSettings.AllowSearchByVendor;
            if (model.asv)
            {
                var vendors = await _vendorService.GetAllVendorsAsync();
                if (vendors.Any())
                {
                    model.AvailableVendors.Add(new SelectListItem
                    {
                        Value = "0",
                        Text = await _localizationService.GetResourceAsync("Common.All")
                    });
                    foreach (var vendor in vendors)
                        model.AvailableVendors.Add(new SelectListItem
                        {
                            Value = vendor.Id.ToString(),
                            Text = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                            Selected = model.vid == vendor.Id
                        });
                }
            }

            model.CatalogProductsModel = await PrepareSearchProductsModelAsync(model, command, searchOption);

            return model;
        }

        /// <summary>
        /// Prepares the search products model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search products model
        /// </returns>
        public virtual async Task<CatalogProductsModel> PrepareSearchProductsModelAsync(SearchModel searchModel, CatalogProductsCommand command, SearchOption? searchOption)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            //sorting
            await _catalogModelFactory.PrepareSortingOptionsAsync(model, command);
            //view mode
            await _catalogModelFactory.PrepareViewModesAsync(model, command);
            //page size
            await _catalogModelFactory.PreparePageSizeOptionsAsync(model, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);

            var searchTerms = searchModel.q == null
                ? string.Empty
                : searchModel.q.Trim();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            //only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            //we don't use "!string.IsNullOrEmpty(searchTerms)" in cases of "ProductSearchTermMinimumLength" set to 0 but searching by other parameters (e.g. category or price filter)
            var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");
            if (isSearchTermSpecified)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.WarningMessage =
                        string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    // Run full-text search and get paged product list
                    products = await _anniqueCustomizationConfigurationService.SearchProductsWithFullTextAsync(
                        pageIndex: command.PageNumber - 1,
                        pageSize: command.PageSize,
                        storeId: currentStore.Id,
                        visibleIndividuallyOnly: true,
                        keywords: searchTerms,
                        orderBy: (ProductSortingEnum)command.OrderBy,
                        searchOption: searchOption
                    );

                    //search term statistics
                    if (!string.IsNullOrEmpty(searchTerms))
                    {
                        var searchTerm =
                            await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, currentStore.Id);
                        if (searchTerm != null)
                        {
                            searchTerm.Count++;
                            await _searchTermService.UpdateSearchTermAsync(searchTerm);
                        }
                        else
                        {
                            searchTerm = new SearchTerm
                            {
                                Keyword = searchTerms,
                                StoreId = currentStore.Id,
                                Count = 1
                            };
                            await _searchTermService.InsertSearchTermAsync(searchTerm);
                        }
                    }

                }
            }

            var isFiltering = !string.IsNullOrEmpty(searchTerms);
            await PrepareCatalogProductsAsync(model, products, isFiltering);

            return model;
        }

        /// <summary>
        /// Prepares catalog products
        /// </summary>
        /// <param name="model">Catalog products model</param>
        /// <param name="products">The products</param>
        /// <param name="isFiltering">A value indicating that filtering has been applied</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareCatalogProductsAsync(CatalogProductsModel model, IPagedList<Product> products, bool isFiltering = false)
        {
            if (!string.IsNullOrEmpty(model.WarningMessage))
                return;

            if (products.Count == 0 && isFiltering)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
            else
            {
                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
                model.LoadPagedList(products);
            }
        }

        private SearchOption? GetParsedSearchOption(string searchOptionValue)
        {
            if (!string.IsNullOrWhiteSpace(searchOptionValue) &&
                Enum.TryParse<SearchOption>(searchOptionValue, true, out var option))
                return option;

            return null;
        }

        #endregion

        #region Searching Methods

        public virtual async Task<IActionResult> Search(SearchModel model, CatalogProductsCommand command)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                store.Id);

            if (model == null)
                model = new SearchModel();

            var searchOptionValue = Request.Query["searchOption"].ToString();

            //parse search option
            var parsedOption = GetParsedSearchOption(searchOptionValue);

            // Set ViewData based on whether searchOption is valid
            ViewData["SearchOption"] = parsedOption?.ToString() ?? "None";
            
            if (parsedOption.HasValue && await _anniqueCustomizationConfigurationService.IsFullTextSearchEnableAsync() && !model.advs)
                //custom prepare search model with full text search SP
                model = await PrepareSearchModelAsync(model, command, parsedOption); 
            else
                //default nopcommerce search
                model = await _catalogModelFactory.PrepareSearchModelAsync(model, command);

            //updated view name to resolve conflict between rich blog search view and this normal search 
            return View("~/Plugins/Annique.Customization/Themes/Avenue/Views/AnniqueCatalog/FullTextSearch.cshtml", model);
        }

        [CheckLanguageSeoCode(ignore: true)]
        public virtual async Task<IActionResult> SearchTermAutoComplete(string term,string searchOption)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Content("");

            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var store = await _storeContext.GetCurrentStoreAsync();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, productNumber); 
            if (searchOption.HasValue() && await _anniqueCustomizationConfigurationService.IsFullTextSearchEnableAsync())
            {

                var parsedOption = GetParsedSearchOption(searchOption);

                products = await _anniqueCustomizationConfigurationService.SearchProductsWithFullTextAsync(0,
                storeId: store.Id,
                keywords: term,
                visibleIndividuallyOnly: true,
                pageSize: productNumber,
                searchOption: parsedOption);
            }
            else 
            {
                products = await _productService.SearchProductsAsync(0,
                storeId: store.Id,
                keywords: term,
                languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);
            }

            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl<Product>(new { SeName = p.SeName }),
                              productpictureurl = p.PictureModels.FirstOrDefault()?.ImageUrl,
                              showlinktoresultsearch = showLinkToResultSearch
                          })
                .ToList();
            return Json(result);
        }

        [CheckLanguageSeoCode(ignore: true)]
        public virtual async Task<IActionResult> SearchProducts(SearchModel searchModel, CatalogProductsCommand command)
        {
            if (searchModel == null)
                searchModel = new SearchModel();

            var searchOptionValue = Request.Query["searchOption"].ToString();

            var parsedOption = GetParsedSearchOption(searchOptionValue);

            // Set ViewData based on whether searchOption is valid
            ViewData["SearchOption"] = parsedOption?.ToString() ?? "None";

            CatalogProductsModel model;
            if (parsedOption.HasValue && await _anniqueCustomizationConfigurationService.IsFullTextSearchEnableAsync() && !searchModel.advs)
                //search with full text search 
                 model = await PrepareSearchProductsModelAsync(searchModel, command, parsedOption);
            else
                //default nopcomerce code
                 model = await _catalogModelFactory.PrepareSearchProductsModelAsync(searchModel, command);
            return PartialView("_ProductsInGridOrLines", model);
        }

        #endregion
    }
}
