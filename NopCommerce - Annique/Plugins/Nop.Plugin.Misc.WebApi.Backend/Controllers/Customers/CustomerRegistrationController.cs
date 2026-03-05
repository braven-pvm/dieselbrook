using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using ChangePasswordRequest = Nop.Services.Customers.ChangePasswordRequest;
using CustomerRegistrationRequest = Nop.Services.Customers.CustomerRegistrationRequest;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerRegistrationController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public CustomerRegistrationController(ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService)
        {
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate customer
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        [HttpGet]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ValidateCustomer([FromQuery, Required] string usernameOrEmail,
            [FromQuery, Required] string password)
        {
            var result = await _customerRegistrationService.ValidateCustomerAsync(usernameOrEmail, password);

            return Ok((int)result);
        }

        /// <summary>
        ///  Register customer
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="email">Email</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="passwordFormat">Password format</param>
        /// <param name="isApproved">Is approved</param>
        [HttpPost("{storeId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Dto.Customers.CustomerRegistrationResult), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RegisterCustomer(int customerId,
            int storeId,
            [FromQuery, Required] string email,
            [FromQuery, Required] string username,
            [FromQuery, Required] string password,
            [FromQuery, Required] PasswordFormat passwordFormat,
            [FromQuery, Required] bool isApproved)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var result = await _customerRegistrationService.RegisterCustomerAsync(new CustomerRegistrationRequest(
                customer, email,
                username, password, passwordFormat, storeId, isApproved));

            return Ok(new Dto.Customers.CustomerRegistrationResult(result));
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="validateRequest">A value indicating whether we should validate request</param>
        /// <param name="passwordFormat">Password format</param>
        /// <param name="newPassword">New password</param>
        /// <param name="oldPassword">Old password</param>
        /// <param name="hashedPasswordFormat">Hashed password format (e.g. SHA1, SHA512)</param>
        [HttpPut]
        [ProducesResponseType(typeof(Dto.Customers.ChangePasswordResult), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ChangePassword([FromQuery, Required] string email,
            [FromQuery, Required] bool validateRequest,
            [FromQuery, Required] PasswordFormat passwordFormat,
            [FromQuery, Required] string newPassword,
            [FromQuery, Required] string oldPassword,
            [FromQuery, Required] string hashedPasswordFormat)
        {
            var result = await _customerRegistrationService.ChangePasswordAsync(new ChangePasswordRequest(email,
                validateRequest, passwordFormat, newPassword, oldPassword, hashedPasswordFormat));

            return Ok(new Dto.Customers.ChangePasswordResult(result));
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="newEmail">New email</param>
        /// <param name="requireValidation">Require validation of new email address</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SetEmail(int customerId,
            [FromQuery, Required] string newEmail,
            [FromQuery, Required] bool requireValidation)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerRegistrationService.SetEmailAsync(customer, newEmail, requireValidation);

            return Ok();
        }

        /// <summary>
        /// Sets a customer username
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="newUsername">New Username</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SetUsername(int customerId, [FromQuery][Required] string newUsername)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerRegistrationService.SetUsernameAsync(customer, newUsername);

            return Ok();
        }

        #endregion
    }
}
