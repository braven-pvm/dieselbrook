using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.WebApi.Framework.Services
{
    public partial interface IJwtTokenService
    {
        /// <summary>
        /// Generate new JWT token
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <returns>JWT token</returns>
        string GetNewJwtToken(Customer customer);

        /// <summary>
        /// Create a new secret key
        /// </summary>
        string NewSecretKey { get; }
    }
}
