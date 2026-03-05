using Nop.Core.Caching;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueCustomization
{
    public interface ICustomCacheManagerService
    {
        Task<CacheKey> PrepareKeyForCustomCacheAsync(CacheKey cacheKey, params object[] cacheKeyParameters);
    }
}
