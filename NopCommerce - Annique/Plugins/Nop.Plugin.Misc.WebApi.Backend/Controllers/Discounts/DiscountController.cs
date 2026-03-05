using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Discounts;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Discounts
{
    public partial class DiscountController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public DiscountController(ICustomerService customerService,
            IDiscountService discountService)
        {
            _customerService = customerService;
            _discountService = discountService;
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

            var discount = await _discountService.GetDiscountByIdAsync(id);

            if (discount == null)
                return NotFound($"Discount Id={id} not found");

            await _discountService.DeleteDiscountAsync(discount);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(id);

            if (discount == null)
                return NotFound($"Discount Id={id} not found");

            return Ok(discount.ToDto<DiscountDto>());
        }

        /// <summary>
        /// Gets all discounts
        /// </summary>
        /// <param name="discountType">Discount type; pass null to load all records</param>
        /// <param name="couponCode">Coupon code to find (exact match); pass null or empty to load all records</param>
        /// <param name="discountName">Discount name; pass null or empty to load all records</param>
        /// <param name="showHidden">A value indicating whether to show expired and not started discounts</param>
        /// <param name="startDateUtc">Discount start date; pass null to load all records</param>
        /// <param name="endDateUtc">Discount end date; pass null to load all records</param>
        /// <param name="isActive">A value indicating whether to get active discounts; "null" to load all discounts; "false" to load only inactive discounts; "true" to load only active discounts</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<DiscountDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] DiscountType? discountType = null,
            [FromQuery] string couponCode = null,
            [FromQuery] string discountName = null,
            [FromQuery] bool showHidden = false,
            [FromQuery] DateTime? startDateUtc = null,
            [FromQuery] DateTime? endDateUtc = null,
            bool? isActive = true)
        {
            var discounts = await _discountService.GetAllDiscountsAsync(discountType, couponCode, discountName,
                showHidden, startDateUtc, endDateUtc, isActive);
            var discountsDto = discounts.Select(d => d.ToDto<DiscountDto>()).ToList();

            return Ok(discountsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DiscountDto model)
        {
            var discount = model.FromDto<Discount>();

            await _discountService.InsertDiscountAsync(discount);

            var discountDto = discount.ToDto<DiscountDto>();

            return Ok(discountDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] DiscountDto model)
        {
            var discount = await _discountService.GetDiscountByIdAsync(model.Id);

            if (discount == null)
                return NotFound($"Discount Id={model.Id} is not found");

            discount = model.FromDto<Discount>();

            await _discountService.UpdateDiscountAsync(discount);

            return Ok();
        }

        #region Validation

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discountId">Discount</param>
        /// <param name="customerId">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate(separator - ;)</param>
        [HttpGet("{discountId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Dto.Discounts.DiscountValidationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ValidateDiscount(int discountId, int customerId, [FromQuery][Required] string couponCodesToValidate)
        {
            var couponCodes = couponCodesToValidate.Split(";").ToArray();

            if (discountId <= 0 || customerId <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            if (discount == null)
                return NotFound($"Discount Id={discountId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={discountId} not found");

            var result = await _discountService.ValidateDiscountAsync(discount, customer, couponCodes);

            return Ok(result);
        }

        #endregion

        #endregion
    }
}
