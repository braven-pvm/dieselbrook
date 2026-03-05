using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Customers;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public CustomerController(IAddressService addressService,
            ICustomerService customerService)
        {
            _addressService = addressService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets online customers
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="ids">A list of customer role identifiers (separator - ;) to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(PagedListDto<Customer, CustomerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOnlineCustomers(string ids,
            [FromQuery, Required] DateTime lastActivityFromUtc,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var customerRoleIds = ids.ToIdArray();
            var customers = await _customerService.GetOnlineCustomersAsync(lastActivityFromUtc, customerRoleIds,
                pageIndex, pageSize);

            return Ok(customers.ToPagedListDto<Customer, CustomerDto>());
        }

        /// <summary>
        /// Gets customers with shopping carts
        /// </summary>
        /// <param name="shoppingCartType">Shopping cart type; pass null to load all records</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="productId">Product identifier; pass null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); pass null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); pass null to load all records</param>
        /// <param name="countryId">Billing country identifier; pass null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Customer, CustomerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomersWithShoppingCarts(
            [FromQuery] ShoppingCartType? shoppingCartType = null,
            [FromQuery] int storeId = 0,
            [FromQuery] int? productId = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] int? countryId = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var customers = await _customerService.GetCustomersWithShoppingCartsAsync(shoppingCartType, storeId,
                productId, createdFromUtc, createdToUtc, countryId, pageIndex, pageSize);

            return Ok(customers.ToPagedListDto<Customer, CustomerDto>());
        }

        /// <summary>
        /// Get customers by identifiers
        /// </summary>
        /// <param name="ids">Array of customer identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<CustomerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomersByIds(string ids)
        {
            var customersId = ids.ToIdArray();
            var customers = await _customerService.GetCustomersByIdsAsync(customersId);

            var customersDto = customers.Select(c => c.ToDto<CustomerDto>());

            return Ok(customersDto);
        }

        /// <summary>
        /// Get customers by guids
        /// </summary>
        /// <param name="customerGuids">Customer guids</param>
        [HttpGet("{customerGuids}")]
        [ProducesResponseType(typeof(IList<CustomerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomersByGuids(string customerGuids)
        {
            var customersGuid = customerGuids.ToGuidArray();

            var customers = await _customerService.GetCustomersByGuidsAsync(customersGuid);

            var customersDto = customers.Select(c => c.ToDto<CustomerDto>());

            return Ok(customersDto);
        }

        /// <summary>
        /// Returns a list of guids of not existing customers
        /// </summary>
        /// <param name="customerGuids">The guids of the customers to check</param>
        [HttpGet("{customerGuids}")]
        [ProducesResponseType(typeof(IList<Guid>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNotExistingCustomers(string customerGuids)
        {
            var customersGuid = customerGuids.ToGuidArray();

            var guids = await _customerService.GetNotExistingCustomersAsync(customersGuid);

            return Ok(guids.ToList());
        }

        /// <summary>
        /// Gets a customer by GUID
        /// </summary>
        /// <param name="guid">Customer GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerByGuid([FromQuery][Required] Guid guid)
        {
            var customer = await _customerService.GetCustomerByGuidAsync(guid);

            if (customer == null)
                return NotFound($"Customer guid={guid} not found");

            return Ok(customer.ToDto<CustomerDto>());
        }

        /// <summary>
        /// Get customer by email
        /// </summary>
        /// <param name="email">Email</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerByEmail([FromQuery][Required] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            var customer = await _customerService.GetCustomerByEmailAsync(email);

            if (customer == null)
                return NotFound($"Customer email={email} not found");

            return Ok(customer.ToDto<CustomerDto>());
        }

        /// <summary>
        /// Get customer by system role
        /// </summary>
        /// <param name="systemName">System name</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerBySystemName([FromQuery][Required] string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return BadRequest();

            var customer = await _customerService.GetCustomerBySystemNameAsync(systemName);

            if (customer == null)
                return NotFound($"Customer system name={systemName} not found");

            return Ok(customer.ToDto<CustomerDto>());
        }

        /// <summary>
        /// Get customer by username
        /// </summary>
        /// <param name="username">Username</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerByUsername([FromQuery][Required] string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest();

            var customer = await _customerService.GetCustomerByUsernameAsync(username);

            if (customer == null)
                return NotFound($"Customer username={username} not found");

            return Ok(customer.ToDto<CustomerDto>());
        }

        /// <summary>
        /// Reset data required for checkout
        /// </summary>
        /// <param name="customerId">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
        /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        [HttpGet("{customerId}/{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ResetCheckoutData(int customerId,
            int storeId,
            [FromQuery] bool clearCouponCodes = false,
            [FromQuery] bool clearCheckoutAttributes = false,
            [FromQuery] bool clearRewardPoints = true,
            [FromQuery] bool clearShippingMethod = true,
            [FromQuery] bool clearPaymentMethod = true)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerService.ResetCheckoutDataAsync(customer, storeId, clearCouponCodes,
                clearCheckoutAttributes, clearRewardPoints, clearShippingMethod,
                clearPaymentMethod);

            return Ok();
        }

        /// <summary>
        /// Delete guest customer records
        /// </summary>
        [HttpGet("onlyWithoutShoppingCart")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DeleteGuestCustomers([FromQuery] DateTime? createdFromUtc,
            [FromQuery] DateTime? createdToUtc,
            bool onlyWithoutShoppingCart)
        {
            var count = await _customerService.DeleteGuestCustomersAsync(createdFromUtc, createdToUtc,
                onlyWithoutShoppingCart);

            return Ok(count);
        }

        /// <summary>
        /// Gets a default tax display type (if configured)
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int?), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerDefaultTaxDisplayType(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var type = await _customerService.GetCustomerDefaultTaxDisplayTypeAsync(customer);

            return Ok((int?)type);
        }

        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerFullName(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var fullName = await _customerService.GetCustomerFullNameAsync(customer);

            return Ok(fullName);
        }

        /// <summary>
        /// Formats the customer name
        /// </summary>
        /// <param name="customerId">Source</param>
        /// <param name="stripTooLong">Strip too long customer name</param>
        /// <param name="maxLength">Maximum customer name length</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatUsername(int customerId,
            [FromQuery] bool stripTooLong = false,
            [FromQuery] int maxLength = 0)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var username = await _customerService.FormatUsernameAsync(customer, stripTooLong, maxLength);

            return Ok(username);
        }

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseAppliedDiscountCouponCodes(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var codes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);

            return Ok(codes);
        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="couponCode">Coupon code</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ApplyDiscountCouponCode(int customerId, [FromQuery][Required] string couponCode)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerService.ApplyDiscountCouponCodeAsync(customer, couponCode);

            return Ok();
        }

        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="couponCode">Coupon code to remove</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RemoveDiscountCouponCode(int customerId, [FromQuery][Required] string couponCode)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerService.RemoveDiscountCouponCodeAsync(customer, couponCode);

            return Ok();
        }

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ParseAppliedGiftCardCouponCodes(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var codes = await _customerService.ParseAppliedGiftCardCouponCodesAsync(customer);

            return Ok(codes);
        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="couponCode">Coupon code to remove</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ApplyGiftCardCouponCode(int customerId, [FromQuery][Required] string couponCode)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerService.ApplyGiftCardCouponCodeAsync(customer, couponCode);

            return Ok();
        }

        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="couponCode">Coupon code to remove</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveGiftCardCouponCode(int customerId, [FromQuery][Required] string couponCode)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            await _customerService.RemoveGiftCardCouponCodeAsync(customer, couponCode);

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound($"Customer Id={id} not found");

            await _customerService.DeleteCustomerAsync(customer);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound($"Customer Id={id} not found");

            return Ok(customer.ToDto<CustomerDto>());
        }

        /// <summary>
        /// Gets all customers
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="lastActivityFromUtc">Last activity date from (UTC); null to load all records</param>
        /// <param name="lastActivityToUtc">Last activity date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="customerRoleIds">A list of customer role identifiers (separator - ;) to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="email">Email; null to load all customers</param>
        /// <param name="username">Username; null to load all customers</param>
        /// <param name="firstName">First name; null to load all customers</param>
        /// <param name="lastName">Last name; null to load all customers</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all customers</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all customers</param>
        /// <param name="company">Company; null to load all customers</param>
        /// <param name="phone">Phone; null to load all customers</param>
        /// <param name="zipPostalCode">Phone; null to load all customers</param>
        /// <param name="ipAddress">IP address; null to load all customers</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Customer, CustomerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            DateTime? lastActivityFromUtc = null, 
            DateTime? lastActivityToUtc = null,
            [FromQuery] int affiliateId = 0,
            [FromQuery] int vendorId = 0,
            [FromQuery] string customerRoleIds = null,
            [FromQuery] string email = null,
            [FromQuery] string username = null,
            [FromQuery] string firstName = null,
            [FromQuery] string lastName = null,
            [FromQuery] int dayOfBirth = 0,
            [FromQuery] int monthOfBirth = 0,
            [FromQuery] string company = null,
            [FromQuery] string phone = null,
            [FromQuery] string zipPostalCode = null,
            [FromQuery] string ipAddress = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool getOnlyTotalCount = false)
        {
            var ids = !string.IsNullOrEmpty(customerRoleIds) ? customerRoleIds.ToIdArray() : null;

            var customers = await _customerService.GetAllCustomersAsync(createdFromUtc, createdToUtc,
                lastActivityFromUtc, lastActivityToUtc, affiliateId,
                vendorId,
                ids, email, username, firstName, lastName, dayOfBirth, monthOfBirth, company, phone, zipPostalCode,
                ipAddress, pageIndex, pageSize, getOnlyTotalCount);

            return Ok(customers.ToPagedListDto<Customer, CustomerDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CustomerDto model)
        {
            var customer = model.FromDto<Customer>();

            await _customerService.InsertCustomerAsync(customer);

            var customerDto = customer.ToDto<CustomerDto>();

            return Ok(customerDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CustomerDto model)
        {
            var customer = await _customerService.GetCustomerByIdAsync(model.Id);

            if (customer == null)
                return NotFound($"Customer Id={model.Id} is not found");

            customer = model.FromDto<Customer>();

            await _customerService.UpdateCustomerAsync(customer);

            return Ok();
        }

        #region Customer address mapping

        /// <summary>
        /// Gets a list of addresses mapped to customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(IList<AddressDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAddressesByCustomerId(int customerId)
        {
            var addresses = await _customerService.GetAddressesByCustomerIdAsync(customerId);

            return Ok(addresses.Select(a => a.ToDto<AddressDto>()).ToList());
        }

        /// <summary>
        /// Gets a address mapped to customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{customerId}/{addressId}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerAddress(int customerId, int addressId)
        {
            var address = await _customerService.GetCustomerAddressAsync(customerId, addressId);

            return Ok(address.ToDto<AddressDto>());
        }

        /// <summary>
        /// Gets a customer billing address
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerBillingAddress(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var address = await _customerService.GetCustomerBillingAddressAsync(customer);

            return Ok(address.ToDto<AddressDto>());
        }

        /// <summary>
        /// Gets a customer shipping address
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomerShippingAddress(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var address = await _customerService.GetCustomerShippingAddressAsync(customer);

            return Ok(address.ToDto<AddressDto>());
        }

        /// <summary>
        /// Remove a customer-address mapping record
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{customerId}/{addressId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveCustomerAddress(int customerId, int addressId)
        {
            if (customerId <= 0 || addressId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var address = await _addressService.GetAddressByIdAsync(addressId);

            if (address == null)
                return NotFound($"Address Id={customerId} not found");

            await _customerService.RemoveCustomerAddressAsync(customer, address);

            return Ok();
        }

        /// <summary>
        /// Inserts a customer-address mapping record
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{customerId}/{addressId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertCustomerAddress(int customerId, int addressId)
        {
            if (customerId <= 0 || addressId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var address = await _addressService.GetAddressByIdAsync(addressId);

            if (address == null)
                return NotFound($"Address Id={customerId} not found");

            await _customerService.InsertCustomerAddressAsync(customer, address);

            return Ok();
        }

        #endregion

        #endregion
    }
}
