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
    public partial class DiscountCategoryMappingController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IRepository<DiscountCategoryMapping> _discountCategoryMappingRepository;

        #endregion

        #region Ctor

        public DiscountCategoryMappingController(ICategoryService categoryService,
            ICustomerService customerService,
            IDiscountService discountService,
            IRepository<DiscountCategoryMapping> discountCategoryMappingRepository)
        {
            _categoryService = categoryService;
            _customerService = customerService;
            _discountService = discountService;
            _discountCategoryMappingRepository = discountCategoryMappingRepository;
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

            var discountCategoryMapping = await _discountCategoryMappingRepository.GetByIdAsync(id);

            if (discountCategoryMapping == null)
                return NotFound($"Discount category mapping Id={id} not found");

            await _categoryService.DeleteDiscountCategoryMappingAsync(discountCategoryMapping);

            return Ok();
        }

        /// <summary>
        /// Get a discount-category mapping record
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="discountId">Discount identifier</param>
        [HttpGet("{categoryId}/{discountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DiscountCategoryMappingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetDiscountAppliedToCategory(int categoryId, int discountId)
        {
            if (categoryId <= 0)
                return BadRequest();

            if (discountId <= 0)
                return BadRequest();

            var discountCategoryMapping = await _categoryService.GetDiscountAppliedToCategoryAsync(categoryId, discountId);

            if (discountCategoryMapping == null)
                return NotFound("Discount category mapping not found");

            return Ok(discountCategoryMapping.ToDto<DiscountCategoryMappingDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountCategoryMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DiscountCategoryMappingDto model)
        {
            var discountCategoryMapping = model.FromDto<DiscountCategoryMapping>();

            await _categoryService.InsertDiscountCategoryMappingAsync(discountCategoryMapping);

            var discountCategoryMappingDto = discountCategoryMapping.ToDto<DiscountCategoryMappingDto>();

            return Ok(discountCategoryMappingDto);
        }

        /// <summary>
        /// Clean up category references for a specified discount
        /// </summary>
        /// <param name="discountId">Discount Id</param>
        [HttpGet("{discountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ClearDiscountCategoryMapping(int discountId)
        {
            if (discountId <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            if (discount == null)
                return NotFound($"Discount Id={discountId} not found");

            await _categoryService.ClearDiscountCategoryMappingAsync(discount);

            return Ok();
        }

        /// <summary>
        /// Get category identifiers to which a discount is applied
        /// </summary>
        /// <param name="discountId">Discount Id</param>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{discountId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAppliedCategoryIds(int discountId, int customerId)
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

            var ids = await _categoryService.GetAppliedCategoryIdsAsync(discount, customer);

            return Ok(ids);
        }

        #endregion
    }
}
