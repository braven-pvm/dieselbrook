using Annique.Plugins.Nop.Customization.Domain.ShippingRule;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.DiscountAllocation.Caching
{
    public class CustomShippingRuleCacheEventConsumer : CacheEventConsumer<CustomShippingByWeightByTotalRecord>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(CustomShippingByWeightByTotalRecord entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update || entityEventType == EntityEventType.Insert)
            {
                await RemoveAsync(AnniqueCustomizationDefaults.CustomShippingByWeightByTotalAllKey);
            }
            await RemoveByPrefixAsync(AnniqueCustomizationDefaults.CUSTOMSHIPPINGBYWEIGHTBYTOTAL_PATTERN_KEY);
        }
    }
}
