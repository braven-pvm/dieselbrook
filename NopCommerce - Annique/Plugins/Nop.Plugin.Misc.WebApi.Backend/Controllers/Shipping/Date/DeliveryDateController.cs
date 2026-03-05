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
    public partial class DeliveryDateController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IDateRangeService _dateRangeService;

        #endregion

        #region Ctor

        public DeliveryDateController(IDateRangeService dateRangeService)
        {
            _dateRangeService = dateRangeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all delivery dates
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<DeliveryDateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var deliveryDates = await _dateRangeService.GetAllDeliveryDatesAsync();

            var deliveryDatesDto = deliveryDates.Select(date => date.ToDto<DeliveryDateDto>()).ToList();

            return Ok(deliveryDatesDto);
        }

        /// <summary>
        /// Get a delivery date
        /// </summary>
        /// <param name="id">The delivery date identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DeliveryDateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(id);

            if (deliveryDate == null)
            {
                return NotFound($"Delivery date Id={id} not found");
            }

            return Ok(deliveryDate.ToDto<DeliveryDateDto>());
        }

        /// <summary>
        /// Create a delivery date
        /// </summary>
        /// <param name="model">Delivery date Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(DeliveryDateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DeliveryDateDto model)
        {
            var deliveryDate = model.FromDto<DeliveryDate>();

            await _dateRangeService.InsertDeliveryDateAsync(deliveryDate);

            return Ok(deliveryDate.ToDto<DeliveryDateDto>());
        }

        /// <summary>
        /// Update a delivery date
        /// </summary>
        /// <param name="model">Delivery date Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] DeliveryDateDto model)
        {
            var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(model.Id);

            if (deliveryDate == null)
                return NotFound("Delivery date is not found");

            deliveryDate = model.FromDto<DeliveryDate>();
            await _dateRangeService.UpdateDeliveryDateAsync(deliveryDate);

            return Ok();
        }

        /// <summary>
        /// Delete a delivery date
        /// </summary>
        /// <param name="id">Delivery date identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(id);

            if (deliveryDate == null)
                return NotFound($"Delivery date Id={id} not found");

            await _dateRangeService.DeleteDeliveryDateAsync(deliveryDate);

            return Ok();
        }

        #endregion
    }
}
