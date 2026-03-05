using Nop.Core.Caching;
using XcellenceIT.Plugin.ProductRibbons.Domain;

namespace XcellenceIT.Plugin.ProductRibbons.Services
{
    public static class ProductRibbonDefaults
    {
        public static CacheKey XITProductRibbons_License => new("XITProductRibbons_LicenseStatus");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public static CacheKey ProductRibbonsAllCacheKey => new("XITProductRibbons.all.{0}", NopEntityCacheDefaults<ProductRibbonRecord>.AllPrefix);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : ribbon ID
        /// </remarks>
        public static CacheKey ProductPictureByRibbonCacheKey => new("XITProductPicture.byribbon.{0}");
        
        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : ribbon id
        /// </remarks>
        public static CacheKey ProductRibbonsMappingsCacheKey => new("XITProductRibbonsMappings.all.{0}", NopEntityCacheDefaults<ProductRibbonMapping>.AllPrefix);

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picture id
        /// {1} : store id
        /// </remarks>
        public static CacheKey ProductRibbonPictureByIdCacheKey => new("XITProductRibbonsPictureById.{0}-{1}");

    }
}
