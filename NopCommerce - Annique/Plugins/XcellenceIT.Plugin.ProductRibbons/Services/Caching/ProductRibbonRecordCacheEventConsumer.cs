using Nop.Core.Caching;
using Nop.Services.Caching;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;

namespace XcellenceIT.Plugin.ProductRibbons.Services.Caching
{
    public class ProductRibbonRecordCacheEventConsumer : CacheEventConsumer<ProductRibbonRecord>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(ProductRibbonRecord entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update || entityEventType == EntityEventType.Insert)
                await RemoveByPrefixAsync(NopEntityCacheDefaults<ProductRibbonRecord>.AllPrefix);

            await RemoveByPrefixAsync(NopEntityCacheDefaults<ProductRibbonRecord>.AllPrefix);

            await RemoveAsync(ProductRibbonDefaults.XITProductRibbons_License);
        }
    }
}
