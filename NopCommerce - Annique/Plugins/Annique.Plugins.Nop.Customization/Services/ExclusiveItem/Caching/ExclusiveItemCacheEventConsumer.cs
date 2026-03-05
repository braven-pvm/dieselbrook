using Annique.Plugins.Nop.Customization.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ExclusiveItem.Caching
{
    public class ExclusiveItemCacheEventConsumer : CacheEventConsumer<Product>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Product entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update || entityEventType == EntityEventType.Insert)
            {
                await RemoveByPrefixAsync(NopEntityCacheDefaults<ExclusiveItems>.AllPrefix);
            }
            await RemoveByPrefixAsync(NopEntityCacheDefaults<ExclusiveItems>.AllPrefix);
        }
    }
}
