using System;
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
    public partial class ReturnRequestController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IReturnRequestService _returnRequestService;

        #endregion

        #region Ctor

        public ReturnRequestController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a return request
        /// </summary>
        /// <param name="id">Return request identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);

            if (returnRequest == null)
                return NotFound($"Return request Id={id} not found");

            await _returnRequestService.DeleteReturnRequestAsync(returnRequest);

            return Ok();
        }

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="id">Return request identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);

            if (returnRequest == null)
            {
                return NotFound($"Return request Id={id} not found");
            }

            return Ok(returnRequest.ToDto<ReturnRequestDto>());
        }

        /// <summary>
        /// Search return requests
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; 0 to load all entries</param>
        /// <param name="orderItemId">Order item identifier; 0 to load all entries</param>
        /// <param name="customNumber">Custom number; null or empty to load all entries</param>
        /// <param name="rs">Return request status; null to load all entries</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ReturnRequest, ReturnRequestDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Search([FromQuery] int storeId = 0,
            [FromQuery] int customerId = 0,
            [FromQuery] int orderItemId = 0,
            [FromQuery] string customNumber = "",
            [FromQuery] ReturnRequestStatus? rs = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool getOnlyTotalCount = false)
        {
            var returnRequests = await _returnRequestService.SearchReturnRequestsAsync(storeId, customerId, orderItemId,
                customNumber, rs, createdFromUtc, createdToUtc,
                pageIndex, pageSize, getOnlyTotalCount);

            var returnRequestsDto = returnRequests.ToPagedListDto<ReturnRequest, ReturnRequestDto>();

            return Ok(returnRequestsDto);
        }

        /// <summary>
        /// Create a return request
        /// </summary>
        /// <param name="model">Return request Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(ReturnRequestDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ReturnRequestDto model)
        {
            var returnRequest = model.FromDto<ReturnRequest>();

            await _returnRequestService.InsertReturnRequestAsync(returnRequest);

            return Ok(returnRequest.ToDto<ReturnRequestDto>());
        }

        /// <summary>
        /// Update a return request
        /// </summary>
        /// <param name="model">Return request Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ReturnRequestDto model)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);

            if (returnRequest == null)
                return NotFound("Return request is not found");

            returnRequest = model.FromDto<ReturnRequest>();
            await _returnRequestService.UpdateReturnRequestAsync(returnRequest);

            return Ok();
        }

        #endregion
    }
}
