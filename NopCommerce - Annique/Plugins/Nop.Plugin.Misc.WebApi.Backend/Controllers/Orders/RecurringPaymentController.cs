using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class RecurringPaymentController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public RecurringPaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="id">Recurring payment identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RecurringPaymentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(id);

            if (recurringPayment == null)
            {
                return NotFound($"Recurring payment Id={id} not found");
            }

            return Ok(recurringPayment.ToDto<RecurringPaymentDto>());
        }

        /// <summary>
        /// Delete a recurring payment
        /// </summary>
        /// <param name="id">Recurring payment identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(id);

            if (recurringPayment == null)
                return NotFound($"Recurring payment Id={id} not found");

            await _orderService.DeleteRecurringPaymentAsync(recurringPayment);

            return Ok();
        }

        /// <summary>
        /// Create an recurring payment
        /// </summary>
        /// <param name="model">Recurring payment Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(RecurringPaymentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] RecurringPaymentDto model)
        {
            var recurringPayment = model.FromDto<RecurringPayment>();

            await _orderService.InsertRecurringPaymentAsync(recurringPayment);

            return Ok(recurringPayment.ToDto<RecurringPaymentDto>());
        }

        /// <summary>
        /// Update the recurring payment
        /// </summary>
        /// <param name="model">Recurring payment Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] RecurringPaymentDto model)
        {
            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(model.Id);

            if (recurringPayment == null)
                return NotFound("Recurring payment is not found");

            recurringPayment = model.FromDto<RecurringPayment>();
            await _orderService.UpdateRecurringPaymentAsync(recurringPayment);

            return Ok();
        }

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="storeId">The store identifier; 0 to load all records</param>
        /// <param name="customerId">The customer identifier; 0 to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; 0 to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<RecurringPayment, RecurringPaymentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Search([FromQuery] int storeId = 0,
            [FromQuery] int customerId = 0,
            [FromQuery] int initialOrderId = 0,
            [FromQuery] OrderStatus? initialOrderStatus = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var recurringPayments = await _orderService.SearchRecurringPaymentsAsync(storeId, customerId,
                initialOrderId,
                initialOrderStatus,
                pageIndex, pageSize,
                showHidden);

            var recurringPaymentsDto = recurringPayments.ToPagedListDto<RecurringPayment, RecurringPaymentDto>();

            return Ok(recurringPaymentsDto);
        }

        #endregion
    }
}
