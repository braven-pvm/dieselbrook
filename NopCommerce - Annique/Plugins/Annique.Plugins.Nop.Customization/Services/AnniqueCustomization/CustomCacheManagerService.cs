using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueCustomization
{
    public class CustomCacheManagerService : ICustomCacheManagerService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private string HashAlgorithm => "SHA1";

        #endregion

        public CustomCacheManagerService(ISettingService settingService,
            IStoreContext storeContext)
        {
            _settingService= settingService;
            _storeContext= storeContext;
        }

        protected virtual string CreateIdsHash(IEnumerable<int> ids)
        {
            var identifiers = ids.ToList();

            if (!identifiers.Any())
                return string.Empty;

            var identifiersString = string.Join(", ", identifiers.OrderBy(id => id));
            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(identifiersString), HashAlgorithm);
        }

        protected virtual object CreateCacheKeyParameters(object parameter)
        {
            return parameter switch
            {
                null => "null",
                IEnumerable<int> ids => CreateIdsHash(ids),
                IEnumerable<BaseEntity> entities => CreateIdsHash(entities.Select(entity => entity.Id)),
                BaseEntity entity => entity.Id,
                decimal param => param.ToString(CultureInfo.InvariantCulture),
                _ => parameter
            };
        }

        public virtual async Task<CacheKey> PrepareKeyForCustomCacheAsync(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);
            var key = cacheKey.Create(CreateCacheKeyParameters, cacheKeyParameters);
            key.CacheTime = settings.CustomCacheExpireTime;
            return key;
        }
    }
}
