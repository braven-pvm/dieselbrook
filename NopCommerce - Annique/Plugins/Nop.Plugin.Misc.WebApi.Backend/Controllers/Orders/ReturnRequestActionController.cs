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
    public partial class ReturnRequestActionController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IReturnRequestService _returnRequestService;

        #endregion

        #region Ctor

        public ReturnRequestActionController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a return request action
        /// </summary>
        /// <param name="id">Return request action identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var returnRequestAction = await _returnRequestService.GetReturnRequestActionByIdAsync(id);

            if (returnRequestAction == null)
                return NotFound($"Return request action Id={id} not found");

            await _returnRequestService.DeleteReturnRequestActionAsync(returnRequestAction);

            return Ok();
        }

        /// <summary>
        /// Gets all return request actions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ReturnRequestActionDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var returnRequestActions = await _returnRequestService.GetAllReturnRequestActionsAsync();

            var returnRequestActionsDto = returnRequestActions.Select(rra => rra.ToDto<ReturnRequestActionDto>()).ToList();

            return Ok(returnRequestActionsDto);
        }

        /// <summary>
        /// Gets a return request action
        /// </summary>
        /// <param name="id">Return request action identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ReturnRequestActionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var returnRequestAction = await _returnRequestService.GetReturnRequestActionByIdAsync(id);

            if (returnRequestAction == null)
            {
                return NotFound($"Return request action Id={id} not found");
            }

            return Ok(returnRequestAction.ToDto<ReturnRequestActionDto>());
        }

        /// <summary>
        /// Create a return request action
        /// </summary>
        /// <param name="model">Return request action Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ReturnRequestActionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ReturnRequestActionDto model)
        {
            var returnRequestAction = model.FromDto<ReturnRequestAction>();

            await _returnRequestService.InsertReturnRequestActionAsync(returnRequestAction);

            return Ok(returnRequestAction.ToDto<ReturnRequestActionDto>());
        }

        /// <summary>
        /// Update a return request action
        /// </summary>
        /// <param name="model">Return request action Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ReturnRequestActionDto model)
        {
            var returnRequestAction = await _returnRequestService.GetReturnRequestActionByIdAsync(model.Id);

            if (returnRequestAction == null)
                return NotFound("Return request action is not found");

            returnRequestAction = model.FromDto<ReturnRequestAction>();
            await _returnRequestService.UpdateReturnRequestActionAsync(returnRequestAction);

            return Ok();
        }

        #endregion
    }
}
