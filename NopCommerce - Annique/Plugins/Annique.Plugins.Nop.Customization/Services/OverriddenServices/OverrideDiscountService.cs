using Annique.Plugins.Nop.Customization.Services.AnniqueCustomization;
using Annique.Plugins.Nop.Customization.Services.DiscountAllocation;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.OverriddenServices
{
    public class OverrideDiscountService : DiscountService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IDiscountCustomerMappingService _discountCustomerMappingService;
        private readonly ISettingService _settingService;
        private readonly ICustomCacheManagerService _customCacheManagerService;

        #endregion

        #region Ctor
        public OverrideDiscountService(ICustomerService customerService,
            IDiscountPluginManager discountPluginManager,
            ILocalizationService localizationService, 
            IProductService productService, 
            IRepository<Discount> discountRepository,
            IRepository<DiscountRequirement> discountRequirementRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository, 
            IRepository<Order> orderRepository, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext,
            IDiscountCustomerMappingService discountCustomerMappingService,
            ISettingService settingService,
            ICustomCacheManagerService customCacheManagerService) : base(customerService, 
                discountPluginManager, 
                localizationService, 
                productService, 
                discountRepository, 
                discountRequirementRepository, 
                discountUsageHistoryRepository, 
                orderRepository, 
                staticCacheManager, 
                storeContext)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _discountCustomerMappingService = discountCustomerMappingService;
            _settingService = settingService;
            _customCacheManagerService = customCacheManagerService;
        }

        #endregion

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the discount validation result
        /// </returns>
        public override async Task<DiscountValidationResult> ValidateDiscountAsync(Discount discount, Customer customer, string[] couponCodesToValidate)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            //get Active store Annique Settings
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(store.Id);

            //if settings null or annique plugin not enable then call base service method
            if (settings == null || !settings.IsEnablePlugin)
                return await base.ValidateDiscountAsync(discount, customer, couponCodesToValidate);

            //invalid by default
            var result = new DiscountValidationResult();

            //check discount is active
            if (!discount.IsActive)
                return result;

            //check coupon code
            if (discount.RequiresCouponCode)
            {
                if (string.IsNullOrEmpty(discount.CouponCode))
                    return result;

                if (couponCodesToValidate == null)
                    return result;

                if (!couponCodesToValidate.Any(x => x.Equals(discount.CouponCode, StringComparison.InvariantCultureIgnoreCase)))
                    return result;
            }

            //Do not allow discounts applied to order subtotal or total when a customer has gift cards in the cart.
            //Otherwise, this customer can purchase gift cards with discount and get more than paid ("free money").
            if (discount.DiscountType == DiscountType.AssignedToOrderSubTotal ||
                discount.DiscountType == DiscountType.AssignedToOrderTotal)
            {
                //do not inject IShoppingCartService via constructor because it'll cause circular references
                var shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
                var cart = await shoppingCartService.GetShoppingCartAsync(customer,
                    ShoppingCartType.ShoppingCart, storeId: store.Id);

                var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                if (await _productService.HasAnyGiftCardProductAsync(cartProductIds))
                {
                    result.Errors = new List<string> { await _localizationService.GetResourceAsync("ShoppingCart.Discount.CannotBeUsedWithGiftCards") };
                    return result;
                }
            }

            //check date range
            var now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                var startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                {
                    result.Errors = new List<string> { await _localizationService.GetResourceAsync("ShoppingCart.Discount.NotStartedYet") };
                    return result;
                }
            }

            if (discount.EndDateUtc.HasValue)
            {
                var endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                {
                    result.Errors = new List<string> { await _localizationService.GetResourceAsync("ShoppingCart.Discount.Expired") };
                    return result;
                }
            }

            //discount customer mapping cache key
            var discountCustomerMappingkey = _staticCacheManager.PrepareKeyForDefaultCache(AnniqueCustomizationDefaults.DiscountCustomerMappingByDiscountCacheKey, discount.Id);

            //Check if DiscountId belongs to DiscountCustomerMapping
            var isDiscountCustomerMapping = await _staticCacheManager.GetAsync(discountCustomerMappingkey, async () => await _discountCustomerMappingService.IsExitDiscountCustomerMappingByDiscountIdAsync(discount.Id));

            //if discount belongs to dscount customer mapping then validate limitation and allocation based on mapping table
            if (isDiscountCustomerMapping)
            {
                #region #585 discount voucher custom cache 

                //prepare cache key
                var cacheKey = await _customCacheManagerService.PrepareKeyForCustomCacheAsync(AnniqueCustomizationDefaults.DiscountCustomerMappingsCacheKey, discount.Id, customer.Id);

                #endregion

                //get discount customer mapping
                var discountCustomerMapping = await _staticCacheManager.GetAsync(cacheKey, async () => await _discountCustomerMappingService.GetDiscountCustomerMappingAsync(discount.Id, customer.Id));

                if (discountCustomerMapping == null)
                {
                    result.Errors.Add(await _localizationService.GetResourceAsync("ShoppingCart.Discount.CustomerNotAuthorized"));
                    return result;
                }

                //discount limitation
                switch (discountCustomerMapping.DiscountLimitation)
                {
                    case DiscountLimitationType.NTimesOnly:
                        {
                            if (discountCustomerMapping.NoTimesUsed >= discountCustomerMapping.LimitationTimes)
                                return result;
                        }

                        break;
                    case DiscountLimitationType.NTimesPerCustomer:
                        {
                            if (await _customerService.IsRegisteredAsync(customer))
                            {
                                if (discountCustomerMapping.NoTimesUsed >= discountCustomerMapping.LimitationTimes)
                                {
                                    result.Errors = new List<string> { await _localizationService.GetResourceAsync("ShoppingCart.Discount.CannotBeUsedAnymore") };

                                    return result;
                                }
                            }
                        }

                        break;
                    case DiscountLimitationType.Unlimited:
                    default:
                        break;
                }
            }
            else
            { 
                //discount limitation
                switch (discount.DiscountLimitation)
                {
                    case DiscountLimitationType.NTimesOnly:
                        {
                            var usedTimes = (await GetAllDiscountUsageHistoryAsync(discount.Id, null, null, false, 0, 1)).TotalCount;
                            if (usedTimes >= discount.LimitationTimes)
                                return result;
                        }

                        break;
                    case DiscountLimitationType.NTimesPerCustomer:
                        {
                            if (await _customerService.IsRegisteredAsync(customer))
                            {
                                var usedTimes = (await GetAllDiscountUsageHistoryAsync(discount.Id, customer.Id, null, false, 0, 1)).TotalCount;
                                if (usedTimes >= discount.LimitationTimes)
                                {
                                    result.Errors = new List<string> { await _localizationService.GetResourceAsync("ShoppingCart.Discount.CannotBeUsedAnymore") };

                                    return result;
                                }
                            }
                        }

                        break;
                    case DiscountLimitationType.Unlimited:
                    default:
                        break;
                }
            }
            

            //discount requirements
            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopDiscountDefaults.DiscountRequirementsByDiscountCacheKey, discount);

            var requirements = await _staticCacheManager.GetAsync(key, async () => await GetAllDiscountRequirementsAsync(discount.Id, true));

            //get top-level group
            var topLevelGroup = requirements.FirstOrDefault();
            if (topLevelGroup == null || (topLevelGroup.IsGroup && !(await GetDiscountRequirementsByParentAsync(topLevelGroup)).Any()) || !topLevelGroup.InteractionType.HasValue)
            {
                //there are no requirements, so discount is valid
                result.IsValid = true;

                return result;
            }

            //requirements exist, let's check them
            var errors = new List<string>();

            result.IsValid = await GetValidationResultAsync(requirements, topLevelGroup.InteractionType.Value, customer, errors);

            //set errors if result is not valid
            if (!result.IsValid)
                result.Errors = errors;

            return result;
        }
    }
}
