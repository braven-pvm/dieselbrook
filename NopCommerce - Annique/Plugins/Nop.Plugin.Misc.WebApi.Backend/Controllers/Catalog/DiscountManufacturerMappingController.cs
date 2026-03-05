using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class DiscountManufacturerMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IRepository<DiscountManufacturerMapping> _discountManufacturerMappingRepository;

        #endregion

        #region Ctor

        public DiscountManufacturerMappingController(IManufacturerService manufacturerService,
            ICustomerService customerService,
            IDiscountService discountService,
            IRepository<DiscountManufacturerMapping> discountManufacturerMappingRepository)
        {
            _manufacturerService = manufacturerService;
            _customerService = customerService;
            _discountService = discountService;
            _discountManufacturerMappingRepository = discountManufacturerMappingRepository;
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

            var discountManufacturerMapping = await _discountManufacturerMappingRepository.GetByIdAsync(id);

            if (discountManufacturerMapping == null)
                return NotFound($"Discount Manufacturer mapping Id={id} not found");

            await _manufacturerService.DeleteDiscountManufacturerMappingAsync(discountManufacturerMapping);

            return Ok();
        }

        /// <summary>
        /// Get a discount-manufacturer mapping record
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="discountId">Discount identifier</param>
        [HttpGet("{manufacturerId}/{discountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DiscountManufacturerMappingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetDiscountAppliedToManufacturer(int manufacturerId, int discountId)
        {
            if (manufacturerId <= 0)
                return BadRequest();

            if (discountId <= 0)
                return BadRequest();

            var discountManufacturerMapping = await _manufacturerService.GetDiscountAppliedToManufacturerAsync(manufacturerId, discountId);

            if (discountManufacturerMapping == null)
                return NotFound($"Discount Manufacturer mapping not found");

            return Ok(discountManufacturerMapping.ToDto<DiscountManufacturerMappingDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountManufacturerMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DiscountManufacturerMappingDto model)
        {
            var discountManufacturerMapping = model.FromDto<DiscountManufacturerMapping>();

            await _manufacturerService.InsertDiscountManufacturerMappingAsync(discountManufacturerMapping);

            var discountManufacturerMappingDto = discountManufacturerMapping.ToDto<DiscountManufacturerMappingDto>();

            return Ok(discountManufacturerMappingDto);
        }

        /// <summary>
        /// Clean up manufacturer references for a specified discount
        /// </summary>
        /// <param name="discountId">Discount Id</param>
        [HttpGet("{discountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ClearDiscountManufacturerMapping(int discountId)
        {
            if (discountId <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            if (discount == null)
                return NotFound($"Discount Id={discountId} not found");

            await _manufacturerService.ClearDiscountManufacturerMappingAsync(discount);

            return Ok();
        }

        /// <summary>
        /// Get manufacturer identifiers to which a discount is applied
        /// </summary>
        /// <param name="discountId">Discount Id</param>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{discountId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAppliedManufacturerIds(int discountId, int customerId)
        {
            if (discountId <= 0)
                return BadRequest();

            if (customerId <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            if (discount == null)
                return NotFound($"Discount Id={discountId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var ids = await _manufacturerService.GetAppliedManufacturerIdsAsync(discount, customer);

            return Ok(ids);
        }

        #endregion
    }
}
