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
    public partial class OrderNoteController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public OrderNoteController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a order note
        /// </summary>
        /// <param name="id">Order note identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OrderNoteDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var orderNote = await _orderService.GetOrderNoteByIdAsync(id);

            if (orderNote == null)
            {
                return NotFound($"Order note Id={id} not found");
            }

            return Ok(orderNote.ToDto<OrderNoteDto>());
        }

        /// <summary>
        /// Gets a list notes of order
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="displayToCustomer">Value indicating whether a customer can see a note; pass null to ignore</param>
        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<OrderNoteDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetOrderNotesByOrderId(int orderId, [FromQuery] bool? displayToCustomer = null)
        {
            if (orderId <= 0)
                return BadRequest();

            var orderNotes = await _orderService.GetOrderNotesByOrderIdAsync(orderId, displayToCustomer);

            var orderNotesDto = orderNotes.Select(orderNote => orderNote.ToDto<OrderNoteDto>()).ToList();

            return Ok(orderNotesDto);
        }

        /// <summary>
        /// Delete a order note
        /// </summary>
        /// <param name="id">Order note identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var orderNote = await _orderService.GetOrderNoteByIdAsync(id);

            if (orderNote == null)
                return NotFound($"Order note Id={id} not found");

            await _orderService.DeleteOrderNoteAsync(orderNote);

            return Ok();
        }

        /// <summary>
        ///  Formats the order note text
        /// </summary>
        /// <param name="id">Order note identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> FormatOrderNoteText(int id)
        {
            if (id <= 0)
                return BadRequest();

            var orderNote = await _orderService.GetOrderNoteByIdAsync(id);

            if (orderNote == null)
            {
                return NotFound($"Order note Id={id} not found");
            }

            return Ok(_orderService.FormatOrderNoteText(orderNote));
        }

        /// <summary>
        /// Create an order note
        /// </summary>
        /// <param name="model">Order note Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(OrderNoteDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] OrderNoteDto model)
        {
            var orderNote = model.FromDto<OrderNote>();

            await _orderService.InsertOrderNoteAsync(orderNote);

            return Ok(orderNote.ToDto<OrderNoteDto>());
        }

        #endregion
    }
}
