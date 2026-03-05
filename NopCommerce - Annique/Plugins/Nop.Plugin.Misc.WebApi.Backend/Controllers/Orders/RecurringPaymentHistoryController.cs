using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Orders;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Orders
{
    public partial class RecurringPaymentHistoryController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public RecurringPaymentHistoryController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a recurring payment history by recurring payment
        /// </summary>
        /// <param name="id">Recurring payment identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<RecurringPaymentHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetByRecurringPaymentId(int id)
        {
            if (id <= 0)
                return BadRequest();

            var recurringPayment = await _orderService.GetRecurringPaymentByIdAsync(id);

            if (recurringPayment == null)
            {
                return NotFound($"Recurring payment Id={id} not found");
            }

            var recurringPaymentHistories = await _orderService.GetRecurringPaymentHistoryAsync(recurringPayment);
            var recurringPaymentHistoryDtos = recurringPaymentHistories.Select(rph => rph.ToDto<RecurringPaymentHistoryDto>()).ToList();

            return Ok(recurringPaymentHistoryDtos);
        }

        /// <summary>
        /// Create an recurring payment history entry
        /// </summary>
        /// <param name="model">Recurring payment history Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(RecurringPaymentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] RecurringPaymentHistoryDto model)
        {
            var recurringPaymentHistory = model.FromDto<RecurringPaymentHistory>();

            await _orderService.InsertRecurringPaymentHistoryAsync(recurringPaymentHistory);

            return Ok(recurringPaymentHistory.ToDto<RecurringPaymentHistoryDto>());
        }

        #endregion
    }
}
