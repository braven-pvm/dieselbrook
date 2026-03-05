using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Models;
using Nop.Plugin.Misc.WebApi.Framework.Services;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Backend.Services
{
    public partial class AuthorizationAdminService : IAuthorizationAdminService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IJwtTokenService _jwtTokenService;

        #endregion

        #region Ctor

        public AuthorizationAdminService(CustomerSettings customerSettings,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IJwtTokenService jwtTokenService)
        {
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _jwtTokenService = jwtTokenService;
        }

        #endregion
        
        #region Methods

        public virtual async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request)
        {
            var username = _customerSettings.UsernamesEnabled ? request.Username : request.Email;

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(username, request.Password);

            if (loginResult != CustomerLoginResults.Successful) 
                return null;

            var customer = await (_customerSettings.UsernamesEnabled
                ? _customerService.GetCustomerByUsernameAsync(username)
                : _customerService.GetCustomerByEmailAsync(username));

            _ = await _customerRegistrationService.SignInCustomerAsync(customer, null, false);

            return new AuthenticateResponse(_jwtTokenService.GetNewJwtToken(customer))
            {
                CustomerId = customer.Id,
                Username = _customerSettings.UsernamesEnabled ? customer.Username : customer.Email
            };

        }

        #endregion
    }
}
