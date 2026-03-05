using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Domain.Enums;
using Annique.Plugins.Nop.Customization.Services.ExclusiveItem;
using AutoMapper.Internal;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    /// <summary>
    /// Custom Product service
    /// </summary>
    public class CustomProductService : ProductService
    {
        #region Fields

        private new readonly CatalogSettings _catalogSettings;
        private new readonly CommonSettings _commonSettings;
        private new readonly IAclService _aclService;
        private new readonly ICustomerService _customerService;
        private new readonly IDateRangeService _dateRangeService;
        private new readonly ILanguageService _languageService;
        private new readonly ILocalizationService _localizationService;
        private new readonly IProductAttributeParser _productAttributeParser;
        private new readonly IProductAttributeService _productAttributeService;
        private new readonly IRepository<Category> _categoryRepository;
        private new readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private new readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        private new readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private new readonly IRepository<Manufacturer> _manufacturerRepository;
        private new readonly IRepository<Product> _productRepository;
        private new readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private new readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private new readonly IRepository<ProductCategory> _productCategoryRepository;
        private new readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private new readonly IRepository<ProductPicture> _productPictureRepository;
        private new readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        private new readonly IRepository<ProductReview> _productReviewRepository;
        private new readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        private new readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private new readonly IRepository<ProductTag> _productTagRepository;
        private new readonly IRepository<ProductVideo> _productVideoRepository;
        private new readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private new readonly IRepository<RelatedProduct> _relatedProductRepository;
        private new readonly IRepository<Shipment> _shipmentRepository;
        private new readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        private new readonly IRepository<TierPrice> _tierPriceRepository;
        private new readonly ISearchPluginManager _searchPluginManager;
        private new readonly IStaticCacheManager _staticCacheManager;
        private new readonly IStoreMappingService _storeMappingService;
        private new readonly IStoreService _storeService;
        private new readonly IWorkContext _workContext;
        private new readonly LocalizationSettings _localizationSettings;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IRepository<ExclusiveItems> _exclusiveItemsRepository;
        private readonly IExclusiveItemsService _exclusiveItemsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Gift> _giftItemsRepository;
        private readonly IRepository<Event> _eventRepository;

        #endregion

        #region Ctor

        public CustomProductService(CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IRepository<Category> categoryRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<DiscountProductMapping> discountProductMappingRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductVideo> productVideoRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
            IRepository<TierPrice> tierPriceRepository,
            ISearchPluginManager searchPluginManager,
            IStaticCacheManager staticCacheManager,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings,
            IStoreContext storeContext,
            ISettingService settingService,
            IRepository<ExclusiveItems> exclusiveItemRepository,
            IExclusiveItemsService exclusiveItemsService,
            IHttpContextAccessor httpContextAccessor,
            IRepository<Gift> giftItemsRepository,
            IRepository<Event> eventRepository)
            : base(catalogSettings,
                commonSettings,
                aclService,
                customerService,
                dateRangeService,
                languageService,
                localizationService,
                productAttributeParser,
                productAttributeService,
                categoryRepository,
                crossSellProductRepository,
                discountProductMappingRepository,
                localizedPropertyRepository,
                manufacturerRepository,
                productRepository,
                productAttributeCombinationRepository,
                productAttributeMappingRepository,
                productCategoryRepository,
                productManufacturerRepository,
                productPictureRepository,
                productTagMappingRepository,
                productReviewRepository,
                productReviewHelpfulnessRepository,
                productSpecificationAttributeRepository,
                productTagRepository,
                productVideoRepository,
                productWarehouseInventoryRepository,
                relatedProductRepository,
                shipmentRepository,
                stockQuantityHistoryRepository,
                tierPriceRepository,
                searchPluginManager,
                staticCacheManager,
                storeService,
                storeMappingService,
                workContext,
                localizationSettings)
        {
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _aclService = aclService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _categoryRepository = categoryRepository;
            _crossSellProductRepository = crossSellProductRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productPictureRepository = productPictureRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagRepository = productTagRepository;
            _productVideoRepository = productVideoRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _relatedProductRepository = relatedProductRepository;
            _shipmentRepository = shipmentRepository;
            _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            _tierPriceRepository = tierPriceRepository;
            _searchPluginManager = searchPluginManager;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
            _storeContext = storeContext;
            _settingService = settingService;
            _exclusiveItemsRepository = exclusiveItemRepository;
            _exclusiveItemsService = exclusiveItemsService;
            _httpContextAccessor = httpContextAccessor;
            _giftItemsRepository = giftItemsRepository;
            _eventRepository = eventRepository;
        }

        #endregion

        #region Method 

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public override async Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //If plugin is not enable
            if (!settings.IsEnablePlugin)
                return await base.SearchProductsAsync(pageIndex, pageSize, categoryIds,
                                                         manufacturerIds,
                                                         storeId,
                                                         vendorId,
                                                         warehouseId,
                                                         productType,
                                                         visibleIndividuallyOnly,
                                                         excludeFeaturedProducts,
                                                         priceMin,
                                                         priceMax,
                                                         productTagId,
                                                         keywords,
                                                         searchDescriptions,
                                                         searchManufacturerPartNumber,
                                                         searchSku,
                                                         searchProductTags,
                                                         languageId,
                                                         filteredSpecOptions,
                                                         orderBy,
                                                         showHidden,
                                                         overridePublished);

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            if (!showHidden)
            {
                //apply store mapping constraints
                productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

                //apply ACL constraints
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden ||
                            DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                            DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                    ) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);
                IQueryable<int> productsByKeywords;

                var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(customer, storeId);

                if (activeSearchProvider is not null)
                {
                    productsByKeywords = (await activeSearchProvider.SearchProductsAsync(keywords, searchLocalizedValue)).AsQueryable();
                }
                else
                {
                    productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords)) ||
                            searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords ||
                            searchSku && p.Sku == keywords
                        select p.Id;

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                            from lp in _localizedPropertyRepository.Table
                            let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                            lp.LocaleValue.Contains(keywords)
                            let checkShortDesc = searchDescriptions &&
                                            lp.LocaleKey == nameof(Product.ShortDescription) &&
                                            lp.LocaleValue.Contains(keywords)
                            where
                                lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                            select lp.EntityId);
                    }
                }

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                //search by category name if admin allows
                if (_catalogSettings.AllowCustomersToSearchWithCategoryName)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                        where c.Name.Contains(keywords)
                        select pc.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join lp in _localizedPropertyRepository.Table on pc.CategoryId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(Category) &&
                              lp.LocaleKey == nameof(Category.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pc.ProductId);
                    }
                }

                //search by manufacturer name if admin allows
                if (_catalogSettings.AllowCustomersToSearchWithManufacturerName)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                        where m.Name.Contains(keywords)
                        select pm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join lp in _localizedPropertyRepository.Table on pm.ManufacturerId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(Manufacturer) &&
                              lp.LocaleKey == nameof(Manufacturer.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pm.ProductId);
                    }
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name.Contains(keywords)
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pptm.ProductId);
                    }
                }

                productsQuery =
                    from p in productsQuery
                    join pbk in productsByKeywords on p.Id equals pbk
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            //Get page url
            var pageUrl = _httpContextAccessor.HttpContext.Request.Path.ToString();

            //Check if not admin area then apply exclusive item filter
            if (!pageUrl.Contains("/Admin/"))
            {
                var anniqueSettings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                //exclusive to you category product ids
                var exclusiveProductIds = _productCategoryRepository.Table
               .Where(pc => pc.CategoryId == anniqueSettings.ExclusiveItemsCategoryId)
               .Select(pc => (int?)pc.ProductId)
               .ToList();

                // get gift productIds from gift table
                var giftProductIds = _giftItemsRepository.Table.Select(gi => (int?)gi.ProductId).Distinct().ToList();

                //get event productIds from event table
                var eventProductIds = _eventRepository.Table.Select(ae => ae.ProductID).Distinct().ToList();

                // prepare excluded product id list (exclusive to you category product ids + gift product ids + event product ids)
                var excludeProductIds = exclusiveProductIds
                                        .Concat(giftProductIds.Cast<int?>())
                                        .Concat(eventProductIds.Cast<int?>())
                                        .Distinct()
                                        .ToList();

                //any exclude product ids
                if (excludeProductIds.Any())
                    // Exclude products with invalid exclusive productIds and gift products id from productsQuery
                    productsQuery = productsQuery.Where(p => !excludeProductIds.Contains(p.Id));
            }

            return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
