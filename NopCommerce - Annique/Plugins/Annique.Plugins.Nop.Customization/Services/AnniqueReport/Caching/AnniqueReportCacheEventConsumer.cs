using Annique.Plugins.Nop.Customization.Domain.AnniqueReports;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueReport.Caching
{
    public class AnniqueReportCacheEventConsumer : CacheEventConsumer<Report>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Report entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Delete || entityEventType == EntityEventType.Update)
            {
                await RemoveAsync(AnniqueCustomizationDefaults.GetReportByIdCacheKey, entity.Id);
                await RemoveAsync(AnniqueCustomizationDefaults.GetPublishedReportsAllCacheKey);
            }
            await RemoveAsync(AnniqueCustomizationDefaults.GetReportByIdCacheKey);
            await RemoveAsync(AnniqueCustomizationDefaults.GetPublishedReportsAllCacheKey);
        }
    }
}
