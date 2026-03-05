using Nop.Core.Caching;

namespace Nop.Plugin.Misc.WebApi.Frontend
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public partial class WebApiFrontendDefaults
    {
        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName => "Misc.WebApi.Frontend";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Misc.WebApi.Frontend.Configure";

        /// <summary>
        /// Gets a key of cache PaymentInfo
        /// </summary>
        /// /// <remarks>
        /// {0} : current store ID
        /// {1} : customer GUID
        /// </remarks>
        public static CacheKey PaymentInfoKeyCache => new CacheKey("Nop.Plugin.Misc.WebApi.Frontend.PaymentInfo-{0}-{1}");
    }
}
