using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Shipping.Date;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Shipping.Date;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Shipping.Date
{
    public partial class ProductAvailabilityRangeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IDateRangeService _dateRangeService;

        #endregion

        #region Ctor

        public ProductAvailabilityRangeController(IDateRangeService dateRangeService)
        {
            _dateRangeService = dateRangeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all product availability ranges
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ProductAvailabilityRangeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var productAvailabilityRange = await _dateRangeService.GetAllProductAvailabilityRangesAsync();

            var productAvailabilityRangeDto = productAvailabilityRange.Select(par => par.ToDto<ProductAvailabilityRangeDto>()).ToList();

            return Ok(productAvailabilityRangeDto);
        }

        /// <summary>
        /// Get all product availability ranges
        /// </summary>
        /// <param name="id">The product availability range identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductAvailabilityRangeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(id);

            if (productAvailabilityRange == null)
            {
                return NotFound($"The product availability range Id={id} not found");
            }

            return Ok(productAvailabilityRange.ToDto<ProductAvailabilityRangeDto>());
        }

        /// <summary>
        /// Create a product availability range
        /// </summary>
        /// <param name="model">Product availability range Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ProductAvailabilityRangeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductAvailabilityRangeDto model)
        {
            var productAvailabilityRange = model.FromDto<ProductAvailabilityRange>();

            await _dateRangeService.InsertProductAvailabilityRangeAsync(productAvailabilityRange);

            return Ok(productAvailabilityRange.ToDto<ProductAvailabilityRangeDto>());
        }

        /// <summary>
        /// Update a product availability range
        /// </summary>
        /// <param name="model">Product availability range Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductAvailabilityRangeDto model)
        {
            var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(model.Id);

            if (productAvailabilityRange == null)
                return NotFound("Product availability range is not found");

            productAvailabilityRange = model.FromDto<ProductAvailabilityRange>();
            await _dateRangeService.UpdateProductAvailabilityRangeAsync(productAvailabilityRange);

            return Ok();
        }

        /// <summary>
        /// Delete a product availability range
        /// </summary>
        /// <param name="id">Product availability range identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(id);

            if (productAvailabilityRange == null)
                return NotFound($"Product availability range Id={id} not found");

            await _dateRangeService.DeleteProductAvailabilityRangeAsync(productAvailabilityRange);

            return Ok();
        }

        #endregion
    }
}
