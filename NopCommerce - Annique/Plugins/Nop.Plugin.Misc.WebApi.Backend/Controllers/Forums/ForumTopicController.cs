using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Forums;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Forums;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Forums;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Forums
{
    public partial class ForumTopicController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public ForumTopicController(IForumService forumService)
        {
            _forumService = forumService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Moves the forum topic
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="newForumId">New forum identifier</param>
        [HttpGet("{forumTopicId}/{newForumId}")]
        [ProducesResponseType(typeof(ForumTopicDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> MoveTopic(int forumTopicId, int newForumId)
        {
            var topic = await _forumService.MoveTopicAsync(forumTopicId, newForumId);

            return Ok(topic.ToDto<ForumTopicDto>());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forumTopic = await _forumService.GetTopicByIdAsync(id);

            if (forumTopic == null)
                return NotFound($"Forum topic Id={id} not found");

            await _forumService.DeleteTopicAsync(forumTopic);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ForumTopicDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forumTopic = await _forumService.GetTopicByIdAsync(id);

            if (forumTopic == null)
                return NotFound($"Forum topic Id={id} not found");

            return Ok(forumTopic.ToDto<ForumTopicDto>());
        }

        /// <summary>
        /// Gets all forum topics
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchType">Search type</param>
        /// <param name="limitDays">Limit by the last number days; 0 to load all topics</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ForumTopic, ForumTopicDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int forumId = 0,
            [FromQuery] int customerId = 0,
            [FromQuery] string keywords = "",
            [FromQuery] ForumSearchType searchType = ForumSearchType.All,
            [FromQuery] int limitDays = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var forumTopics = await _forumService.GetAllTopicsAsync(forumId, customerId, keywords, searchType,
                limitDays, pageIndex, pageSize);

            return Ok(forumTopics.ToPagedListDto<ForumTopic, ForumTopicDto>());
        }

        /// <summary>
        /// Gets active forum topics
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ForumTopic, ForumTopicDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetActiveTopics([FromQuery] int forumId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var forumTopics = await _forumService.GetActiveTopicsAsync(forumId, pageIndex, pageSize);

            return Ok(forumTopics.ToPagedListDto<ForumTopic, ForumTopicDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ForumTopicDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ForumTopicDto model)
        {
            var forumTopic = model.FromDto<ForumTopic>();

            await _forumService.InsertTopicAsync(forumTopic, true);

            var forumTopicDto = forumTopic.ToDto<ForumTopicDto>();

            return Ok(forumTopicDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ForumTopicDto model)
        {
            var forumTopic = await _forumService.GetTopicByIdAsync(model.Id);

            if (forumTopic == null)
                return NotFound($"Forum topic Id={model.Id} is not found");

            forumTopic = model.FromDto<ForumTopic>();

            await _forumService.UpdateTopicAsync(forumTopic);

            return Ok();
        }

        #endregion
    }
}
