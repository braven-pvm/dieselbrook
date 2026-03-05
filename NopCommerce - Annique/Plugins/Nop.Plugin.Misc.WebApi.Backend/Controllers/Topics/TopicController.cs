using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Topics;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Topics;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Topics;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Topics
{
    public partial class TopicController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ITopicService _topicService;

        #endregion

        #region Ctor

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create topic
        /// </summary>
        /// <param name="model">Topic Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(TopicDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] TopicDto model)
        {
            var topic = model.FromDto<Topic>();

            await _topicService.InsertTopicAsync(topic);

            var topicDto = topic.ToDto<TopicDto>();

            return Ok(topicDto);
        }

        /// <summary>
        /// Update topic by Id
        /// </summary>
        /// <param name="model">Topic Dto</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] TopicDto model)
        {
            var topic = await _topicService.GetTopicByIdAsync(model.Id);

            if (topic == null)
                return NotFound("Topic is not found");

            topic = model.FromDto<Topic>();
            await _topicService.UpdateTopicAsync(topic);

            return Ok();
        }

        /// <summary>
        /// Delete topic
        /// </summary>
        /// <param name="id">Topic identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var topic = await _topicService.GetTopicByIdAsync(id);

            if (topic == null)
                return NotFound($"Topic Id={id} not found");

            await _topicService.DeleteTopicAsync(topic);

            return Ok();
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="id">The topic identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var topic = await _topicService.GetTopicByIdAsync(id);

            if (topic == null)
            {
                return NotFound($"Topic Id={id} not found");
            }

            var topicDto = topic.ToDto<TopicDto>();

            return Ok(topicDto);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <param name="storeId">Store identifier; pass 0 to ignore filtering by store and load the first one</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySystemName([FromQuery, Required] string systemName,
            [FromQuery] int storeId = 0)
        {
            var topic = await _topicService.GetTopicBySystemNameAsync(systemName, storeId);

            if (topic == null)
                return NotFound($"Topic systemName={systemName} not found");

            var topicDto = topic.ToDto<TopicDto>();

            return Ok(topicDto);
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="keywords">Keywords to search into body or title</param>
        /// <param name="ignoreAcl">A value indicating whether to ignore ACL rules</param>
        /// <param name="showHidden">A value indicating whether to show hidden topics</param>
        /// <param name="onlyIncludedInTopMenu">A value indicating whether to show only topics which include on the top menu</param>HttpGet("{storeId}")]
        [HttpGet("{storeId}")]
        [ProducesResponseType(typeof(IList<TopicDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int storeId,
            [FromQuery] string keywords = null,
            [FromQuery] bool ignoreAcl = false,
            [FromQuery] bool showHidden = false,
            [FromQuery] bool onlyIncludedInTopMenu = false)
        {
            IList<Topic> topics;
            if (string.IsNullOrEmpty(keywords))
                topics = await _topicService.GetAllTopicsAsync(storeId, ignoreAcl, showHidden, onlyIncludedInTopMenu);
            else
                topics = await _topicService.GetAllTopicsAsync(storeId, keywords, ignoreAcl, showHidden,
                    onlyIncludedInTopMenu);

            var topicsDto = topics.Select(topic => topic.ToDto<TopicDto>()).ToList();

            return Ok(topicsDto);
        }

        #endregion
    }
}
