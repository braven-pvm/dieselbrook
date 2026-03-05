using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Domain.AnniqueEvents;
using Annique.Plugins.Nop.Customization.Domain.AnniqueGifts;
using Annique.Plugins.Nop.Customization.Domain.Enums;
using Annique.Plugins.Nop.Customization.Domain.FulltextSearch;
using Annique.Plugins.Nop.Customization.Services.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.SpecialOffers;
using AutoMapper.Internal;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueCustomization
{
    /// <summary>
    /// AnniqueCustomizationConfigurationService Service
    /// </summary>
    public class AnniqueCustomizationConfigurationService : IAnniqueCustomizationConfigurationService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPluginService _pluginService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAwardService _awardService;
        private readonly IAddressService _addressService;
        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IAclService _aclService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductVideo> _productVideoRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        private readonly IRepository<TierPrice> _tierPriceRepository;
        private readonly ISearchPluginManager _searchPluginManager;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IRepository<ExclusiveItems> _exclusiveItemsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Gift> _giftItemsRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IAffiliateService _affiliateService;
        private readonly ISpecialOffersService _specialOffersService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INopDataProvider _nopDataProvider;

        #endregion

        #region Ctor

        public AnniqueCustomizationConfigurationService(ISettingService settingService,
            IStoreContext storeContext,
            IPluginService pluginService,
            IWorkContext workContext,
            ICustomerService customerService,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IAwardService awardService,
            IAddressService addressService,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
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
            LocalizationSettings localizationSettings,
            IRepository<ExclusiveItems> exclusiveItemRepository,
            IHttpContextAccessor httpContextAccessor,
            IRepository<Gift> giftItemsRepository, 
            IRepository<Event> eventRepository,
            IAffiliateService affiliateService,
            ICustomerActivityService customerActivityService,
            ISpecialOffersService specialOffersService,
            INopDataProvider nopDataProvider)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _pluginService = pluginService;
            _workContext = workContext;
            _customerService = customerService;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _awardService = awardService;
            _addressService = addressService;
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _aclService = aclService;
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
            _localizationSettings = localizationSettings;
            _exclusiveItemsRepository = exclusiveItemRepository;
            _httpContextAccessor = httpContextAccessor;
            _giftItemsRepository = giftItemsRepository;
            _eventRepository = eventRepository;
            _affiliateService = affiliateService;
            _specialOffersService = specialOffersService;
            _customerActivityService = customerActivityService;
            _nopDataProvider = nopDataProvider;
        }

        #endregion

        #region Utility Method

        /// <summary>
        ///Return any capital letters exist or not in provided string 
        ///Return True if any capital letter in string,return False if all letters are in smallcase
        /// </summary>
        private bool HasCapitalLetters(string text)
        {
            return text.Any(char.IsUpper);
        }

        #endregion

        #region Methods

        /// <summary>
        ///Return Plugin is enable or disable
        /// </summary>
        public async Task<bool> IsPluginEnableAsync()
        {
            // Get active store
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.AnniqueCustomizationPluginEnableCacheKey, store.Id);

            // Try to get the result from the cache
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                // Get active store Annique settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                // Get plugin descriptor by system name
                var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(
                    "Annique.Customization", LoadPluginsMode.InstalledOnly);

                // Check if the plugin is installed and enabled
                return pluginDescriptor != null && pluginDescriptor.Installed && settings.IsEnablePlugin;
            });
        }

        /// <summary>
        ///Return Pickup collection is enable or disable
        /// </summary>
        public async Task<bool> IsPickupCollectionEnableAsync()
        {
            bool isPickUpCollectionEnable = false;
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            var pluginEnable = await IsPluginEnableAsync();
            if (pluginEnable && settings.IsPickUpCollection)
                isPickUpCollectionEnable = true;

            return isPickUpCollectionEnable;
        }

        /// <summary>
        ///Return Full text search is enable or disable
        /// </summary>
        public async Task<bool> IsFullTextSearchEnableAsync()
        {
            // Get active store
            var store = await _storeContext.GetCurrentStoreAsync();
            var pluginEnable = await IsPluginEnableAsync();
            if (!pluginEnable)
                return false;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.FullTextSearchCacheKey, store.Id);

            // Try to get the result from the cache
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                // Get active store Annique settings
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                // Check if full text search enabled
                return settings.IsFullTextSearchEnabled;
            });
        }

        /// <summary>
        /// Returns Wheather Customer has consultant role
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<bool> IsConsultantRoleAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return false;

            //get active store
            var store = await _storeContext.GetCurrentStoreAsync();

            // Prepare cache key for consultant role check (key should include CustomerId)
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.IsConsultantRoleCacheKey, customer.Id);

            // Retrieve from cache or execute the logic to determine consultant role
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                // Get store-specific settings (consultant role ID)
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

                // Get the customer role IDs
                var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

                // Check if customer contains the Consultant role
                return customerRoleIds.Contains(settings.ConsultantRoleId);
            });
        }

        /// <summary>
        /// Returns shopping cart totals before discounts
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<(decimal cartTotal, string cartTotalValue)> GetShoppingCartTotalsBeforeDiscountAsync(IList<ShoppingCartItem> cart)
        {
            // Early exit if the cart is empty
            // If there are no items in the cart, return zero total and formatted zero value
            if (!cart.Any())
                return (decimal.Zero, await _priceFormatter.FormatPriceAsync(decimal.Zero, false, false));

            // Extract distinct product IDs from the cart items
            var productIds = cart.Select(item => item.ProductId).Distinct().ToArray();

            // Fetch all products using the extracted product IDs
            // This call retrieves product details from the service and converts it to a dictionary for quick lookups
            var products = (await _productService.GetProductsByIdsAsync(productIds)).ToDictionary(p => p.Id);

            // Filter items where AttributesXml is null or empty
            // These items will be included in the total cart calculation
            var itemsToInclude = cart.Where(item => string.IsNullOrEmpty(item.AttributesXml)).ToList();

            // Filter items where AttributesXml is not null or empty
            // These items may contain special offers or awards that need further processing
            var attributeXmlItems = cart.Where(item => !string.IsNullOrEmpty(item.AttributesXml)).ToList();

            // Initialize an empty list for special offer items
            var specialOfferItems = Enumerable.Empty<ShoppingCartItem>();

            #region Task 642 Special offer on order total

            // Step 2: Items with AttributesXml that contain a non-fully-free SpecialOffer
            specialOfferItems = cart.Where(item =>
            {
                var xml = item.AttributesXml;
                if (string.IsNullOrEmpty(xml))
                    return false;

                var status = _specialOffersService.ParseSpecialOfferStatus(xml);
                return status.HasSpecialOffer && !status.IsFullyFree;
            });

            #endregion

            // Combine items with empty or null AttributesXml and the processed special offer items
            // The Union operation ensures that there are no duplicates based on default equality comparison
            var combinedItems = itemsToInclude
                .Union(specialOfferItems)
                .ToList();

            // Calculate the total cart value
            // Sum the prices of the combined items based on their quantities
            var cartTotal = combinedItems
               .Sum(sci =>
               {
                   // Lookup the product details from the dictionary
                   if (products.TryGetValue(sci.ProductId, out var product))
                   {
                       // Calculate the total price for the item
                       return product.Price * sci.Quantity;
                   }

                   // Return zero if the product is not found in the dictionary
                   return 0m;
               });

            // Format the cart total into a human-readable string
            var cartTotalValue = await _priceFormatter.FormatPriceAsync(cartTotal, false, false);

            // Return the calculated cart total and its formatted string representation
            return (cartTotal, cartTotalValue);
        }

        /// <summary>
        /// Validate and update Billing Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ValidateBillingAddress(Address address)
        {
            //flag to check address updated or not
            var addressUpdated = false;

            //Check for if email not null OR email has capital letters
            if (!string.IsNullOrEmpty(address.Email) && HasCapitalLetters(address.Email))
            {
                // Update billing address email to lowercase
                address.Email = address.Email.ToLower();
                addressUpdated = true;
            }

            // Check for if phone number is not null & Not starts with "+27"
            if (!string.IsNullOrEmpty(address.PhoneNumber) && !address.PhoneNumber.StartsWith("+27"))
            {
                //Apped prefix for phone number
                address.PhoneNumber = "+27" + address.PhoneNumber;
                addressUpdated = true;
            }

            //if address fields updated then update records in table
            if (addressUpdated)
                await _addressService.UpdateAddressAsync(address);
        }

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
        public async Task<IPagedList<Product>> SearchProductsAsync(
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
            AnniqueProductSortingEnum orderBy = AnniqueProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

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
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku == keywords)
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
                            DisplayOrder = pc.First().DisplayOrder
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
                            DisplayOrder = pm.First().DisplayOrder
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
                var giftProductIds = _giftItemsRepository.Table.Select(gi => (int?)gi.ProductId).ToList();

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

        /// <summary>
        /// Returns customized customer name based on affiliate and customer role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetCustomizedCustomerFullNameAsync(Customer customer)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            //if admin nothing to display
            if (customerRoles.Any(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName))
                return string.Empty;

            //get customer full name
            var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
            
            if (!customerRoles.Any(cr => cr.Id == settings.ConsultantRoleId) && customer.AffiliateId != 0)
            {
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(customer.AffiliateId);
                if (affiliate != null)
                {
                    var affiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                    customerFullName = string.Format(await _localizationService.GetResourceAsync("AffiliateWelcomeMessage.Header"),affiliateName,customerFullName);
                }
            }

            return customerFullName;
        }

        /// <summary>
        /// sets customer role
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SetCustomerRoleToRegisteredUserAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            // Fetch the customer roles
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);

            // Check if the customer only has the 'Registered' role
            if (customerRoles.Count == 1 && customerRoles.Any(role => role.SystemName == NopCustomerDefaults.RegisteredRoleName))
            {
                // Fetch the 'Customer' role
                var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync("Customer");
                if (customerRole != null)
                {
                    // Add the 'Customer' role to the customer
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                }
            }
        }

        /// <summary>
        /// get username for share link
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetUsernameAsync()
        {
            // Fetch the current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            // Check if customer is null
            if (customer == null)
                return string.Empty;

            // Check if the customer has a consultant role
            if (!await IsConsultantRoleAsync())
                return string.Empty;

            //cache key for username lookup
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.GetUsernameByCustomerIdCacheKey, customer.Id);

            return await _staticCacheManager.GetAsync(cacheKey, () =>
            {
                return customer.Username;
            });
        }

        /// <summary>
        /// sets Client role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SetClientRoleToUserAsync(Customer customer)
        {
            //if customer has no affiliate then only add client role
            if (customer.AffiliateId == 0)
            { 
                // Fetch the 'Client' role
                var clientRole = await _customerService.GetCustomerRoleBySystemNameAsync(AnniqueCustomizationDefaults.ClientRole);
                if (clientRole != null)
                {
                    // Add the 'Client' role 
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = clientRole.Id });
                }
            }
        }

        private string SanitizeSearchKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return string.Empty;

            // Convert to lowercase for uniformity
            keyword = keyword.ToLowerInvariant();

            // Remove anything that's not a letter, digit, or space
            keyword = Regex.Replace(keyword, @"[^a-z0-9\s]", "");

            // Remove specific reserved words like 'and'
            var reservedWords = new[] { "and" };
            foreach (var word in reservedWords)
            {
                keyword = Regex.Replace(keyword, $@"\b{word}\b", "", RegexOptions.IgnoreCase);
            }

            // Replace multiple spaces with a single space
            keyword = Regex.Replace(keyword, @"\s+", " ").Trim();

            return keyword;
        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="keywords">Keywords</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public async Task<IPagedList<Product>> SearchProductsWithFullTextAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int storeId = 0,
            bool visibleIndividuallyOnly = false,
            string keywords = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            SearchOption? searchOption = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;
            
            List<int> matchedProductIds = null;
            if (!string.IsNullOrEmpty(keywords))
            {
                int fullTextMode = searchOption switch
                {
                    SearchOption.AnyWords => 5,
                    SearchOption.AllWords => 10,
                    _ => 10 // fallback to and
                };

                var cleanKeyword = SanitizeSearchKeyword(keywords);

                var fullTextResults = await _nopDataProvider.QueryProcAsync<FullTextSearchProducts>(
                                        "ANQ_SearchFulltext",
                                        new DataParameter { Name = "Keywords", Value = cleanKeyword },
                                        new DataParameter { Name = "FullTextMode", Value = fullTextMode }
                                    );

                matchedProductIds = fullTextResults.Select(r => r.ProductID).Distinct().ToList();

                if (!matchedProductIds.Any())
                    return new PagedList<Product>(new List<Product>(), pageIndex, pageSize);

                productsQuery = productsQuery.Where(p => matchedProductIds.Contains(p.Id));
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
          
            productsQuery = productsQuery.Where(p => p.Published);
            
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
           
            productsQuery =
            from p in productsQuery
            where !p.Deleted &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                     DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) 
                select p;

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
                var giftProductIds = _giftItemsRepository.Table.Select(gi => (int?)gi.ProductId).ToList();

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

        /// <summary>
        /// Show trip promotion 
        /// </summary>
        /// <param name="totalRsp">Total rsp</param>
        /// <returns>This task returns to show promotion message or not , also return promotion message </returns>

        public async Task<(bool ShowBox, string PromotionMessage)> ShouldShowTripPromotionAsync(decimal totalRsp)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            if (!settings.IsTripEnable || totalRsp == decimal.Zero)
                return (false, string.Empty);

            var currentUser = await _workContext.GetCurrentCustomerAsync();

            if(currentUser == null)
                return (false, string.Empty);

            // Get customer roles, make sure it's not null
            var roles = await _customerService.GetCustomerRolesAsync(currentUser);
            if (roles == null || !roles.Any())
                return (false, string.Empty);

            // Check if the customer role is "Client"
            var hasClientRole = roles
                .Any(role => role.SystemName.Equals("Client", StringComparison.OrdinalIgnoreCase));

            if (!hasClientRole)
                return (false, string.Empty);

            // Check if the customer's account creation date is within the promotion window
            if (currentUser.CreatedOnUtc < settings.TripStartDate || currentUser.CreatedOnUtc > settings.TripEndDate)
                return (false, string.Empty); // If account creation date is outside the promotion window, don't show promotion

            if (totalRsp > settings.QualifyingAmount)
                return (false,  string.Empty);

            var remainingAmountDecimal = Math.Round(settings.QualifyingAmount - totalRsp, 2);
            if(remainingAmountDecimal <= 0)
                return (false, string.Empty);

            var remaningAmount  = await _priceFormatter.FormatPriceAsync(remainingAmountDecimal, true, false);

            string messageTemplate = settings.TripMessageTemplate;

            string promotionMessage = string.IsNullOrWhiteSpace(messageTemplate)
                ? $"You need to order {remaningAmount} more to qualify for the Mediterranean Competition."
                : messageTemplate.Replace("%amount%", remaningAmount);

            return (true, promotionMessage);
        }
    }
}
