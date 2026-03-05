using Nop.Services.Caching;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;

namespace XcellenceIT.Plugin.ProductRibbons.Services.Caching
{
    public class ProductPictureRibbonCacheEventConsumer : CacheEventConsumer<ProductPictureRibbon>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(ProductPictureRibbon entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update || entityEventType == EntityEventType.Insert)
            {
                await RemoveAsync(ProductRibbonDefaults.ProductPictureByRibbonCacheKey, entity.RibbonId);
                await RemoveAsync(ProductRibbonDefaults.ProductRibbonsMappingsCacheKey, entity.RibbonId);
                await RemoveAsync(ProductRibbonDefaults.ProductRibbonPictureByIdCacheKey);
            }

            await RemoveAsync(ProductRibbonDefaults.ProductPictureByRibbonCacheKey, entity.RibbonId);
            await RemoveAsync(ProductRibbonDefaults.ProductRibbonsMappingsCacheKey, entity.RibbonId);
            await RemoveAsync(ProductRibbonDefaults.ProductRibbonPictureByIdCacheKey);
        }
    }
}
