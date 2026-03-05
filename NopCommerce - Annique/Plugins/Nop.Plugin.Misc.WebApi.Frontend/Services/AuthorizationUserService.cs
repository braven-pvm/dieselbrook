using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Models;
using Nop.Plugin.Misc.WebApi.Framework.Services;
using Nop.Plugin.Misc.WebApi.Frontend.Models;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Frontend.Services
{
    public partial class AuthorizationUserService : IAuthorizationUserService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public AuthorizationUserService(
            CustomerSettings customerSettings,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IJwtTokenService jwtTokenService,
            IWorkContext workContext
            )
        {
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _jwtTokenService = jwtTokenService;
            _workContext = workContext;
        }

        #endregion

        #region Utilites

        private AuthenticateResponse GenerateJwtToken(Customer customer)
        {
            return new(_jwtTokenService.GetNewJwtToken(customer))
            {
                CustomerId = customer.Id,
                Username = _customerSettings.UsernamesEnabled ? customer.Username : customer.Email
            };
        }

        #endregion

        #region Methods

        public virtual async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateCustomerRequest request)
        {
            Customer customer;

            if (request.IsGuest)
            {
                customer = await _customerService.InsertGuestCustomerAsync();
                await _workContext.SetCurrentCustomerAsync(customer);

                return GenerateJwtToken(customer);
            }

            var username = _customerSettings.UsernamesEnabled ? request.Username : request.Email;

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(username, request.Password);

            if (loginResult == CustomerLoginResults.Successful)
            {
                customer = await(_customerSettings.UsernamesEnabled
                    ? _customerService.GetCustomerByUsernameAsync(username)
                    : _customerService.GetCustomerByEmailAsync(username));

                _ = await _customerRegistrationService.SignInCustomerAsync(customer, null);

                return GenerateJwtToken(customer);
            }

            return null;
        }

        #endregion
    }
}
