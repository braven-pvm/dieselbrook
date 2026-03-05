using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Customers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Customers
{
    public partial class CustomerAttributeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerAttributeService _customerAttributeService;

        #endregion

        #region Ctor

        public CustomerAttributeController(ICustomerAttributeService customerAttributeService)
        {
            _customerAttributeService = customerAttributeService;
        }

        #endregion

        #region Methods
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customerAttribute = await _customerAttributeService.GetCustomerAttributeByIdAsync(id);

            if (customerAttribute == null)
                return NotFound($"Customer attribute Id={id} not found");

            await _customerAttributeService.DeleteCustomerAttributeAsync(customerAttribute);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customerAttribute = await _customerAttributeService.GetCustomerAttributeByIdAsync(id);

            if (customerAttribute == null)
                return NotFound($"Customer attribute Id={id} not found");

            return Ok(customerAttribute.ToDto<CustomerAttributeDto>());
        }

        /// <summary>
        /// Gets all customer attributes
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IList<CustomerAttributeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var customerAttributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
            var customerAttributesDto = customerAttributes.Select(a => a.ToDto<CustomerAttributeDto>());

            return Ok(customerAttributesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerAttributeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CustomerAttributeDto model)
        {
            var customerAttribute = model.FromDto<CustomerAttribute>();

            await _customerAttributeService.InsertCustomerAttributeAsync(customerAttribute);

            var customerAttributeDto = customerAttribute.ToDto<CustomerAttributeDto>();

            return Ok(customerAttributeDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CustomerAttributeDto model)
        {
            var customerAttribute = await _customerAttributeService.GetCustomerAttributeByIdAsync(model.Id);

            if (customerAttribute == null)
                return NotFound($"Customer attribute Id={model.Id} is not found");

            customerAttribute = model.FromDto<CustomerAttribute>();

            await _customerAttributeService.UpdateCustomerAttributeAsync(customerAttribute);

            return Ok();
        }

        #endregion
    }
}
