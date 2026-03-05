using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Polls;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Polls;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Polls;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Polls
{
    public partial class PollController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPollService _pollService;

        #endregion

        #region Ctor

        public PollController(IPollService pollService)
        {
            _pollService = pollService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all polls
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; pass 0 to load all records</param>
        /// <param name="showHidden">Whether to show hidden records (not published, not started and expired)</param>
        /// <param name="loadShownOnHomepageOnly">Retrieve only shown on home page polls</param>
        /// <param name="systemKeyword">The poll system keyword; pass null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{storeId}")]
        [ProducesResponseType(typeof(PagedListDto<Poll, PollDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int storeId,
            [FromQuery] int languageId = 0,
            [FromQuery] bool showHidden = false,
            [FromQuery] bool loadShownOnHomepageOnly = false,
            [FromQuery] string systemKeyword = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var polls = await _pollService.GetPollsAsync(storeId, languageId, showHidden,
                loadShownOnHomepageOnly, systemKeyword,
                pageIndex, pageSize);

            var pollsDto = polls.ToPagedListDto<Poll, PollDto>();

            return Ok(pollsDto);
        }

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="id">Poll identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var poll = await _pollService.GetPollByIdAsync(id);

            if (poll == null)
            {
                return NotFound($"Poll Id={id} not found");
            }

            return Ok(poll.ToDto<PollDto>());
        }

        /// <summary>
        /// Create a poll
        /// </summary>
        /// <param name="model">Poll Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PollDto model)
        {
            var poll = model.FromDto<Poll>();

            await _pollService.InsertPollAsync(poll);

            return Ok(poll.ToDto<PollDto>());
        }

        /// <summary>
        /// Update a poll
        /// </summary>
        /// <param name="model">Poll Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] PollDto model)
        {
            var poll = await _pollService.GetPollByIdAsync(model.Id);

            if (poll == null)
                return NotFound("Poll is not found");

            poll = model.FromDto<Poll>();
            await _pollService.UpdatePollAsync(poll);

            return Ok();
        }

        /// <summary>
        /// Delete a poll
        /// </summary>
        /// <param name="id">Poll identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var poll = await _pollService.GetPollByIdAsync(id);

            if (poll == null)
                return NotFound($"Poll Id={id} not found");

            await _pollService.DeletePollAsync(poll);

            return Ok();
        }

        #endregion
    }
}
