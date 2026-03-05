using Nop.Core.Caching;
using Nop.Core.Domain.Discounts;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.DiscountAllocation.Caching
{
    public class DiscountCustomerMappingCacheEventConsumer : CacheEventConsumer<Discount>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Discount entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update || entityEventType == EntityEventType.Insert)
            {
                await RemoveAsync(AnniqueCustomizationDefaults.OfferListAllCacheKey);
                await RemoveAsync(AnniqueCustomizationDefaults.ActiveSpecialOffersAllCacheKey);
                await RemoveAsync(AnniqueCustomizationDefaults.OfferBgImageCacheKey);
            }
            await RemoveByPrefixAsync(AnniqueCustomizationDefaults.DiscountMappingsPrefix);
            await RemoveByPrefixAsync(NopEntityCacheDefaults<DiscountCategoryMapping>.AllPrefix);
        }
    }
}
