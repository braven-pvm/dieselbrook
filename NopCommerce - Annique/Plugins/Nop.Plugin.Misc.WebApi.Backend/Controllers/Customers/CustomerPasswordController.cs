using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerPasswordController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IRepository<CustomerPassword> _customerPasswordRepository;

        #endregion

        #region Ctor

        public CustomerPasswordController(ICustomerService customerService,
            IRepository<CustomerPassword> customerPasswordRepository)
        {
            _customerService = customerService;
            _customerPasswordRepository = customerPasswordRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get current customer password
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(CustomerPasswordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetCurrentPassword(int customerId)
        {
            return Ok(await _customerService.GetCurrentPasswordAsync(customerId));
        }

        /// <summary>
        /// Check whether password recovery token is valid
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="token">Token to validate</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsPasswordRecoveryTokenValid(int customerId, [FromQuery][Required] string token)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token));
        }

        /// <summary>
        /// Check whether password recovery link is expired
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsPasswordRecoveryLinkExpired(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer));
        }

        /// <summary>
        /// Check whether customer password is expired 
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsPasswordExpired(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsPasswordExpiredAsync(customer));
        }

        /// <summary>
        /// Gets customer passwords
        /// </summary>
        /// <param name="customerId">Customer identifier; pass null to load all records</param>
        /// <param name="passwordFormat">Password format; pass null to load all records</param>
        /// <param name="passwordsToReturn">Number of returning passwords; pass null to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<CustomerPasswordDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int? customerId = null,
            [FromQuery] PasswordFormat? passwordFormat = null,
            [FromQuery] int? passwordsToReturn = null)
        {
            var customerPasswords =
                await _customerService.GetCustomerPasswordsAsync(customerId, passwordFormat, passwordsToReturn);

            return Ok(customerPasswords.Select(cp => cp.ToDto<CustomerPasswordDto>()));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerPasswordDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CustomerPasswordDto model)
        {
            var customerPassword = model.FromDto<CustomerPassword>();

            await _customerService.InsertCustomerPasswordAsync(customerPassword);

            var customerPasswordDto = customerPassword.ToDto<CustomerPasswordDto>();

            return Ok(customerPasswordDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CustomerPasswordDto model)
        {
            var customerPassword = await _customerPasswordRepository.GetByIdAsync(model.Id);

            if (customerPassword == null)
                return NotFound($"Customer password Id={model.Id} is not found");

            customerPassword = model.FromDto<CustomerPassword>();

            await _customerService.UpdateCustomerPasswordAsync(customerPassword);

            return Ok();
        }

        #endregion
    }
}
