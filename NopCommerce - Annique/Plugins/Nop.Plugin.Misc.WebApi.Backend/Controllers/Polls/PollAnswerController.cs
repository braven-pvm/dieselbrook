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
    public partial class PollAnswerController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IPollService _pollService;

        #endregion

        #region Ctor

        public PollAnswerController(IPollService pollService)
        {
            _pollService = pollService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a poll answer
        /// </summary>
        /// <param name="id">Poll answer identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PollAnswerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var pollAnswer = await _pollService.GetPollAnswerByIdAsync(id);

            if (pollAnswer == null)
            {
                return NotFound($"Poll answer Id={id} not found");
            }

            return Ok(pollAnswer.ToDto<PollAnswerDto>());
        }

        /// <summary>
        /// Gets a poll answer
        /// </summary>
        /// <param name="id">Poll identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PagedListDto<PollAnswer, PollAnswerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetByPollId(int id,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            if (id <= 0)
                return BadRequest();

            var pollAnswers = await _pollService.GetPollAnswerByPollAsync(id, pageIndex, pageSize);
            var pollAnswerDto = pollAnswers.ToPagedListDto<PollAnswer, PollAnswerDto>();

            return Ok(pollAnswerDto);
        }

        /// <summary>
        /// Create a poll answer
        /// </summary>
        /// <param name="model">Poll answer Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(PollAnswerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PollAnswerDto model)
        {
            var pollAnswer = model.FromDto<PollAnswer>();

            await _pollService.InsertPollAnswerAsync(pollAnswer);

            return Ok(pollAnswer.ToDto<PollAnswerDto>());
        }

        /// <summary>
        /// Update a poll answer
        /// </summary>
        /// <param name="model">Poll answer Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] PollAnswerDto model)
        {
            var pollAnswer = await _pollService.GetPollAnswerByIdAsync(model.Id);

            if (pollAnswer == null)
                return NotFound("Poll answer is not found");

            pollAnswer = model.FromDto<PollAnswer>();
            await _pollService.UpdatePollAnswerAsync(pollAnswer);

            return Ok();
        }

        /// <summary>
        /// Delete a poll answer
        /// </summary>
        /// <param name="id">Poll answer identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var pollAnswer = await _pollService.GetPollAnswerByIdAsync(id);

            if (pollAnswer == null)
                return NotFound($"Poll answer Id={id} not found");

            await _pollService.DeletePollAnswerAsync(pollAnswer);

            return Ok();
        }

        #endregion
    }
}
