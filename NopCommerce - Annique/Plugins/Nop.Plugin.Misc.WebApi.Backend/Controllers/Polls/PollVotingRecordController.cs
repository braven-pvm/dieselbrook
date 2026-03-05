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
    public partial class PollVotingRecordController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPollService _pollService;

        #endregion

        #region Ctor

        public PollVotingRecordController(IPollService pollService)
        {
            _pollService = pollService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a poll voting records by parent answer
        /// </summary>
        /// <param name="id">Poll answer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PagedListDto<PollVotingRecord, PollVotingRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetByPollAnswerId(int id, [FromQuery]int pageIndex = 0, [FromQuery] int pageSize = int.MaxValue)
        {
            if (id <= 0)
                return BadRequest();

            var records = await _pollService.GetPollVotingRecordsByPollAnswerAsync(id, pageIndex, pageSize);
            var recordsDto = records.ToPagedListDto<PollVotingRecord, PollVotingRecordDto>();

            return Ok(recordsDto);
        }

        /// <summary>
        /// Create a poll voting record
        /// </summary>
        /// <param name="model">Poll voting record Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(PollAnswerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PollVotingRecordDto model)
        {
            var pollVotingRecord = model.FromDto<PollVotingRecord>();

            await _pollService.InsertPollVotingRecordAsync(pollVotingRecord);

            return Ok(pollVotingRecord.ToDto<PollVotingRecordDto>());
        }

        /// <summary>
        /// Gets a value indicating whether customer already voted for this poll
        /// </summary>
        /// <param name="pollId">Poll identifier</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{pollId}/{customerId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AlreadyVoted(int pollId, int customerId)
        {
            if (pollId <= 0 || customerId <= 0)
                return BadRequest();

            var result = await _pollService.AlreadyVotedAsync(pollId, customerId);

            return Ok(result);
        }

        #endregion
    }
}
