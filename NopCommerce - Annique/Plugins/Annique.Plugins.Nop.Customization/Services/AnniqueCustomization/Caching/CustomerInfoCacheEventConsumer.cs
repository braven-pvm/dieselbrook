using Nop.Core.Domain.Customers;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueCustomization.Caching
{
    public class CustomerInfoCacheEventConsumer : CacheEventConsumer<Customer>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Customer entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update || entityEventType == EntityEventType.Insert)
            {
                await RemoveAsync(AnniqueCustomizationDefaults.GetUsernameByCustomerIdCacheKey,entity.Id);
                await RemoveAsync(AnniqueCustomizationDefaults.IsConsultantRoleCacheKey, entity.Id);
            }
            await RemoveAsync(AnniqueCustomizationDefaults.GetUsernameByCustomerIdCacheKey,entity.Id);
            await RemoveAsync(AnniqueCustomizationDefaults.IsConsultantRoleCacheKey, entity.Id);
            await RemoveAsync(AnniqueCustomizationDefaults.ChatbotCustomerAcesssCacheKey, entity.Id);
        }
    }
}
