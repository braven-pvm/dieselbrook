using Microsoft.IdentityModel.Tokens;

namespace Nop.Plugin.Misc.WebApi.Framework
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class WebApiCommonDefaults
    {
        /// <summary>
        /// Gets customer key of http context
        /// </summary>
        public static string CustomerKey => "nopApiUser";

        /// <summary>
        /// Gets Claim type
        /// </summary>
        public static string ClaimTypeName => "CustomerId";

        /// <summary>
        /// Gets SystemName of main API plugin
        /// </summary>
        public static string MainSystemName => "Misc.WebApi.Frontend";

        /// <summary>
        /// Gets the RoutePrefix of the Swagger UI
        /// </summary>
        public static string SwaggerUIRoutePrefix => "api";

        /// <summary>
        /// Gets the name of the header to be used for security
        /// </summary>
        public static string SecurityHeaderName => "Authorization";

        /// <summary>
        /// Token lifetime in days
        /// </summary>
        public static int TokenLifeTime => 7;

        /// <summary>
        /// The JWT token signature algorithm
        /// </summary>
        public static string JwtSignatureAlgorithm => SecurityAlgorithms.HmacSha256;

        /// <summary>
        /// The minimal length of secret key applied to signature algorithm
        /// <remarks>
        /// For HmacSha256 it may be at least 16 (128 bites)
        /// </remarks>
        /// </summary>
        public static int MinSecretKeyLength => 16;

        /// <summary>
        /// Gets swagger document version
        /// </summary>
        public const string API_VERSION = "v4.60.02";
    }
}
