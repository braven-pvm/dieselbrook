using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class OverriddenPriceCalculationService : PriceCalculationService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager  _staticCacheManager;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public OverriddenPriceCalculationService(CatalogSettings catalogSettings, 
            CurrencySettings currencySettings, 
            ICategoryService categoryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService,
            IDiscountService discountService, 
            IManufacturerService manufacturerService,
            IProductAttributeParser productAttributeParser, 
            IProductService productService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ISettingService settingService) : base(catalogSettings, 
                currencySettings, 
                categoryService,
                currencyService, 
                customerService, 
                discountService, 
                manufacturerService, 
                productAttributeParser, 
                productService, 
                staticCacheManager)
        {
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _discountService = discountService;
            _productService = productService;
            _storeContext = storeContext;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Adjust Unit price with voucher discounts 
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="appliedDiscounts">Applied discounts list</param>
        /// <param name="unitPriceWithDiscounts">Unit price</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the unit price with standards discounts only
        /// </returns>
        public async Task<decimal> AdjustFinalPriceWithVouchersAsync(int customerId, List<Discount> appliedDiscounts, decimal unitPriceWithDiscounts)
        {
            var _discountCustomerMappingService = EngineContext.Current.Resolve<IDiscountCustomerMappingService>();

            // Get customer discount mappings
            var discountCustomerMappings = await _discountCustomerMappingService.GetAllDiscountCustomerMappingsAsync(customerId);

            if (!discountCustomerMappings.Any() || !appliedDiscounts.Any())
            {
                return unitPriceWithDiscounts;
            }

            decimal finalPrice = unitPriceWithDiscounts;

            var specialDiscountIds = discountCustomerMappings.Select(dc => dc.DiscountId).ToHashSet();

            // Find the special discount from appliedDiscounts
            var specialDiscount = appliedDiscounts.FirstOrDefault(d => specialDiscountIds.Contains(d.Id));
            if (specialDiscount != null)
            {
                finalPrice += specialDiscount.DiscountAmount;
            }

            return finalPrice;
        }

        /// <summary>
        /// Gets Cheapest product from cart
        /// </summary>
        /// <param name="discountAppliedProductIds">discount Applied ProductIds</param>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task returns cheapest product's Id from cart
        /// </returns>
        private async Task<int> GetCheapeastProductAsync(IEnumerable<int> discountAppliedProductIds, IList<ShoppingCartItem> cart)
        {
            //get products by ids
            var products = await _productService.GetProductsByIdsAsync(discountAppliedProductIds.ToArray());

            // Check if productsdiscount is null or empty
            if (products == null || !products.Any())
            {
                // no products are found return 0
                return 0;
            }

            // if the cart is null or empty
            if (cart == null || !cart.Any())
            {
                // the cart is empty by returning 0
                return 0;
            }

            var minPrice = products.Max(p => p.Price);
            var result = 0;
            foreach (var item in cart)
            {
                if (products.Any(p => p.Id.Equals(item.ProductId)))
                {
                    (minPrice, result) = products
                        .Where(p => p.Id.Equals(item.ProductId))
                        .FirstOrDefault()
                        .Price <= minPrice ?
                            (products
                                .Where(p => p.Id.Equals(item.ProductId))
                                .FirstOrDefault()
                                .Price, item.ProductId) : (minPrice, result);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets Products ids In Discount
        /// </summary>
        /// <param name="discount">discount</param>
        /// <param name="cart">Cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task returns product's Id from cart and which are connected to discounts
        /// </returns>
        private async Task<IEnumerable<int>> GetProductsInDiscountAsync(Discount discount, IList<ShoppingCartItem> cart)
        {
            if (discount.DiscountType.Equals(DiscountType.AssignedToSkus))
            {
                //get discounted product's Id
                var discountedProduct = (await _productService.GetProductsWithAppliedDiscountAsync(discount.Id)).Select(x => x.Id);
                return discountedProduct.ToList();
            }

            var result = new List<int>();
            if (cart.Count > 0)
            {
                var productIds = cart.Select(x => x.ProductId).ToArray();
                //get product categy ids
                var productCategoriesIds = await _categoryService.GetProductCategoryIdsAsync(productIds);

                var allcategories = new List<int>();
                //get categories where discount is applied
                var categories = (await _categoryService.GetCategoriesByAppliedDiscountAsync(discount.Id)).Select(x => x.Id);
                allcategories.AddRange(categories);
                foreach (var item in categories)
                {
                    //get child category and add into category list
                    allcategories.AddRange(await _categoryService.GetChildCategoryIdsAsync(item));
                }

                //check cart item's any product's categoryId match with discounted category Id
                //If matched then take product id from cart and make a list 
                result = await productCategoriesIds
                            ?.Where(kv => kv.Value.Intersect(allcategories).Any())
                            .Select(kv => kv.Key)
                            .ToListAsync();
            }

            return result.ToHashSet();
        }

        #endregion

        #region Method

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="productPriceWithoutDiscount">Already calculated product price without discount</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the discount amount, Applied discounts
        /// </returns>
        protected override async Task<(decimal, List<Discount>)> GetDiscountAmountAsync(Product product,
            Customer customer,
            decimal productPriceWithoutDiscount)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //If plugin is not enable
            if (!settings.IsEnablePlugin)
                return await base.GetDiscountAmountAsync(product, customer, productPriceWithoutDiscount);

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var appliedDiscounts = new List<Discount>();
            var appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (product.CustomerEntersPrice)
                return (appliedDiscountAmount, appliedDiscounts);

            //discounts are disabled
            if (_catalogSettings.IgnoreDiscounts)
                return (appliedDiscountAmount, appliedDiscounts);

            var allowedDiscounts = await GetAllowedDiscountsAsync(product, customer);

            // Apply only to One Product
            if (allowedDiscounts.Any())
            {
                var shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
                var _discountCustomerMappingService = EngineContext.Current.Resolve<IDiscountCustomerMappingService>();
                var shoppingCartItems = await shoppingCartService.GetShoppingCartAsync(customer, storeId: store.Id);
                var discountToRemove = new HashSet<Discount>();

                //get customer discount mapping
                var discountCustomerMappings = await _discountCustomerMappingService.GetAllDiscountCustomerMappingsAsync(customer.Id);
                if (discountCustomerMappings.Any())
                {
                    // Get the list of discount IDs from discountCustomerMappings
                    var mappedDiscountIds = discountCustomerMappings.Select(mapping => mapping.DiscountId).ToList();

                    //only check for AssignedToSkus and AssignedToCategories as these 2 discounts applied on product level
                    foreach (var discount in allowedDiscounts.Where(d => mappedDiscountIds.Contains(d.Id) && (d.DiscountType == DiscountType.AssignedToSkus || d.DiscountType == DiscountType.AssignedToCategories)))
                    {
                        //get productids connected with discount
                        var discountAppliedProductIds = await GetProductsInDiscountAsync(discount, shoppingCartItems);
                        //get cheapest product 
                        var cheapeastProduct = await GetCheapeastProductAsync(discountAppliedProductIds, shoppingCartItems);
                        //if current product do not match with cheapest then add discount into remove list
                        if (!cheapeastProduct.Equals(0) && !product.Id.Equals(cheapeastProduct))
                            discountToRemove.Add(discount);

                        if (discountToRemove.Any())
                        {
                            allowedDiscounts = allowedDiscounts.Except(discountToRemove).ToList();
                        }
                    }
                }
            }

            //no discounts
            if (!allowedDiscounts.Any())
                return (appliedDiscountAmount, appliedDiscounts);

            appliedDiscounts = _discountService.GetPreferredDiscount(allowedDiscounts, productPriceWithoutDiscount, out appliedDiscountAmount);

            return (appliedDiscountAmount, appliedDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="store">Store</param>
        /// <param name="overriddenProductPrice">Overridden product price. If specified, then it'll be used instead of a product price. For example, used with product attribute combinations</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="rentalStartDate">Rental period start date (for rental products)</param>
        /// <param name="rentalEndDate">Rental period end date (for rental products)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the final price without discounts, Final price, Applied discount amount, Applied discounts
        /// </returns>
        public override async Task<(decimal priceWithoutDiscounts, decimal finalPrice, decimal appliedDiscountAmount, List<Discount> appliedDiscounts)> GetFinalPriceAsync(Product product,
            Customer customer,
            Store store,
            decimal? overriddenProductPrice,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate)
        {
            var _anniqueCustomizationConfigurationService = EngineContext.Current.Resolve<IAnniqueCustomizationConfigurationService>();

            var pluginEnable = await _anniqueCustomizationConfigurationService.IsPluginEnableAsync();

            if (!pluginEnable)
                return await base.GetFinalPriceAsync(product,customer,store,overriddenProductPrice,additionalCharge,includeDiscounts,quantity,rentalStartDate,rentalEndDate);

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductPriceCacheKey,
                product,
                overriddenProductPrice,
                additionalCharge,
                includeDiscounts,
                quantity,
                await _customerService.GetCustomerRoleIdsAsync(customer),
                store);

            //we do not cache price if this not allowed by settings or if the product is rental product
            //otherwise, it can cause memory leaks (to store all possible date period combinations)
            if (!_catalogSettings.CacheProductPrices || product.IsRental)
                cacheKey.CacheTime = 0;

            decimal rezPrice;
            decimal rezPriceWithoutDiscount;
            decimal discountAmount;
            List<Discount> appliedDiscounts;

            (rezPriceWithoutDiscount, rezPrice, discountAmount, appliedDiscounts) = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var discounts = new List<Discount>();
                var appliedDiscountAmount = decimal.Zero;

                //initial price
                var price = overriddenProductPrice ?? product.Price;

                //tier prices
                var tierPrice = await _productService.GetPreferredTierPriceAsync(product, customer, store, quantity);

                if (tierPrice != null)
                    price = tierPrice.Price;

                //additional charge
                price += additionalCharge;

                //rental products
                if (product.IsRental)
                    if (rentalStartDate.HasValue && rentalEndDate.HasValue)
                        price *= _productService.GetRentalPeriods(product, rentalStartDate.Value, rentalEndDate.Value);

                var priceWithoutDiscount = price;

                if (includeDiscounts)
                {
                    //discount
                    var (tmpDiscountAmount, tmpAppliedDiscounts) = await GetDiscountAmountAsync(product, customer, price);
                    price -= tmpDiscountAmount;

                    if (tmpAppliedDiscounts?.Any() ?? false)
                    {
                        #region bug 606 Voucher for 100 % Discount on Product

                        // Calculate price using only standard discount amounts, excluding any voucher discounts.
                        // This is to prevent issues where the unit price could become zero due to excessive discounting from vouchers.
                        // By isolating standard discounts, we ensure that the pricing logic remains valid and avoids unintended zero values.
                        price = await AdjustFinalPriceWithVouchersAsync(customer.Id, tmpAppliedDiscounts, price);

                        #endregion

                        discounts.AddRange(tmpAppliedDiscounts);
                        appliedDiscountAmount = tmpDiscountAmount;
                    }
                }

                if (price < decimal.Zero)
                    price = decimal.Zero;

                if (priceWithoutDiscount < decimal.Zero)
                    priceWithoutDiscount = decimal.Zero;

                return (priceWithoutDiscount, price, appliedDiscountAmount, discounts);
            });

            return (rezPriceWithoutDiscount, rezPrice, discountAmount, appliedDiscounts);
        }

        #endregion
    }
}
