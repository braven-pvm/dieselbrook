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
    public partial class ReturnRequestReasonController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IReturnRequestService _returnRequestService;

        #endregion

        #region Ctor

        public ReturnRequestReasonController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a return request reasons
        /// </summary>
        /// <param name="id">Return request reasons identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var returnRequestReason = await _returnRequestService.GetReturnRequestReasonByIdAsync(id);

            if (returnRequestReason == null)
                return NotFound($"Return request reasons Id={id} not found");

            await _returnRequestService.DeleteReturnRequestReasonAsync(returnRequestReason);

            return Ok();
        }

        /// <summary>
        /// Gets all return request reasons
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ReturnRequestReasonDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var returnRequestReasons = await _returnRequestService.GetAllReturnRequestReasonsAsync();

            var returnRequestReasonDtos = returnRequestReasons.Select(rrr => rrr.ToDto<ReturnRequestReasonDto>()).ToList();

            return Ok(returnRequestReasonDtos);
        }

        /// <summary>
        /// Gets a return request reason
        /// </summary>
        /// <param name="id">Return request reason identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ReturnRequestReasonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var returnRequestReason = await _returnRequestService.GetReturnRequestReasonByIdAsync(id);

            if (returnRequestReason == null)
            {
                return NotFound($"Return request reason Id={id} not found");
            }

            return Ok(returnRequestReason.ToDto<ReturnRequestReasonDto>());
        }

        /// <summary>
        /// Create a return request reason
        /// </summary>
        /// <param name="model">Return request reason Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ReturnRequestReasonDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ReturnRequestReasonDto model)
        {
            var returnRequestReason = model.FromDto<ReturnRequestReason>();

            await _returnRequestService.InsertReturnRequestReasonAsync(returnRequestReason);

            return Ok(returnRequestReason.ToDto<ReturnRequestReasonDto>());
        }

        /// <summary>
        /// Update a return request reason
        /// </summary>
        /// <param name="model">Return request reason Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ReturnRequestReasonDto model)
        {
            var returnRequestReason = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.Id);

            if (returnRequestReason == null)
                return NotFound("Return request reason is not found");

            returnRequestReason = model.FromDto<ReturnRequestReason>();
            await _returnRequestService.UpdateReturnRequestReasonAsync(returnRequestReason);

            return Ok();
        }

        #endregion
    }
}
