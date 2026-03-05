using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerRoleController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public CustomerRoleController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a customer-customer role mapping
        /// </summary>
        [HttpPost]
        public virtual async Task<IActionResult> AddCustomerRoleMapping([FromBody] CustomerCustomerRoleMappingDto model)
        {
            var mapping = model.FromDto<CustomerCustomerRoleMapping>();

            await _customerService.AddCustomerRoleMappingAsync(mapping);

            return Ok();
        }

        /// <summary>
        /// Remove a customer-customer role mapping
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="roleId">Customer role Id</param>
        [HttpGet("{customerId}/{roleId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RemoveCustomerRoleMapping(int customerId, int roleId)
        {
            if (customerId <= 0 || roleId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var customerRole = await _customerService.GetCustomerRoleByIdAsync(roleId);

            if (customerRole == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);

            return Ok();
        }

        /// <summary>
        /// Get customer role identifiers
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetCustomerRoleIds(int customerId, [FromQuery] bool showHidden = false)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var ids = await _customerService.GetCustomerRoleIdsAsync(customer, showHidden);

            return Ok(ids);
        }

        /// <summary>
        /// Gets list of customer roles
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<CustomerRoleDto>), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetCustomerRoles(int customerId, [FromQuery] bool showHidden = false)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var roles = await _customerService.GetCustomerRolesAsync(customer, showHidden);

            return Ok(roles.Select(r => r.ToDto<CustomerRoleDto>()).ToList());
        }

        /// <summary>
        /// Gets a value indicating whether customer is in a certain customer role
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="customerRoleSystemName">Customer role system name</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsInCustomerRole(int customerId,
            [FromQuery, Required] string customerRoleSystemName,
            [FromQuery] bool onlyActiveCustomerRoles = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsInCustomerRoleAsync(customer, customerRoleSystemName,
                onlyActiveCustomerRoles));
        }

        /// <summary>
        /// Gets a value indicating whether customer is administrator
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="onlyActive">A value indicating whether we should look only in active customer roles</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsAdmin(int customerId, [FromQuery] bool onlyActive = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsAdminAsync(customer, onlyActive));
        }

        /// <summary>
        /// Gets a value indicating whether customer is a forum moderator
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="onlyActive">A value indicating whether we should look only in active customer roles</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> IsForumModerator(int customerId, [FromQuery] bool onlyActive = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsForumModeratorAsync(customer, onlyActive));
        }

        /// <summary>
        /// Gets a value indicating whether customer is registered
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="onlyActive">A value indicating whether we should look only in active customer roles</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsRegistered(int customerId, [FromQuery] bool onlyActive = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsRegisteredAsync(customer, onlyActive));
        }

        /// <summary>
        /// Gets a value indicating whether customer is guest
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="onlyActive">A value indicating whether we should look only in active customer roles</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsGuest(int customerId, [FromQuery] bool onlyActive = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsGuestAsync(customer, onlyActive));
        }

        /// <summary>
        /// Gets a value indicating whether customer is vendor
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="onlyActive">A value indicating whether we should look only in active customer roles</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> IsVendor(int customerId, [FromQuery] bool onlyActive = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _customerService.IsVendorAsync(customer, onlyActive));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customerRole = await _customerService.GetCustomerRoleByIdAsync(id);

            if (customerRole == null)
                return NotFound($"Customer role Id={id} not found");

            await _customerService.DeleteCustomerRoleAsync(customerRole);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerRoleDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customerRole = await _customerService.GetCustomerRoleByIdAsync(id);

            if (customerRole == null)
                return NotFound($"Customer role Id={id} not found");

            return Ok(customerRole.ToDto<CustomerRoleDto>());
        }

        /// <summary>
        /// Gets a customer role
        /// </summary>
        /// <param name="systemName">Customer role system name</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerRoleDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerRoleBySystemName([FromQuery][Required] string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return BadRequest();

            var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(systemName);

            if (customerRole == null)
                return NotFound($"Customer system name = {systemName} not found");

            return Ok(customerRole.ToDto<CustomerRoleDto>());
        }

        /// <summary>
        /// Gets all customer roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<CustomerRole, CustomerRoleDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] bool showHidden = false)
        {
            var customerRoles = await _customerService.GetAllCustomerRolesAsync(showHidden);

            return Ok(customerRoles.Select(cr => cr.ToDto<CustomerRoleDto>()).ToList());
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerRoleDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CustomerRoleDto model)
        {
            var customerRole = model.FromDto<CustomerRole>();

            await _customerService.InsertCustomerRoleAsync(customerRole);

            var customerRoleDto = customerRole.ToDto<CustomerRoleDto>();

            return Ok(customerRoleDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CustomerRoleDto model)
        {
            var customerRole = await _customerService.GetCustomerRoleByIdAsync(model.Id);

            if (customerRole == null)
                return NotFound($"Customer role Id={model.Id} is not found");

            customerRole = model.FromDto<CustomerRole>();

            await _customerService.UpdateCustomerRoleAsync(customerRole);

            return Ok();
        }

        #endregion
    }
}
