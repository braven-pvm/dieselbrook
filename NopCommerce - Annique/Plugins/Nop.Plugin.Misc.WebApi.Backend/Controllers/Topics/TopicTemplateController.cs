using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Topics;
using Nop.Plugin.Misc.WebApi.Backend.Dto.TopicTemplates;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Topics;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Topics
{
    public partial class TopicTemplateController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ITopicTemplateService _topicTemplateService;

        #endregion

        #region Ctor

        public TopicTemplateController(ITopicTemplateService topicTemplateService)
        {
            _topicTemplateService = topicTemplateService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all topic templates
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<TopicTemplateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var topicTemplates = await _topicTemplateService.GetAllTopicTemplatesAsync();

            var topicTemplatesDto = topicTemplates.Select(topicTemplate => topicTemplate.ToDto<TopicTemplateDto>()).ToList();

            return Ok(topicTemplatesDto);
        }

        /// <summary>
        /// Gets a topic template
        /// </summary>
        /// <param name="id">Topic template identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var topicTemplate = await _topicTemplateService.GetTopicTemplateByIdAsync(id);

            if (topicTemplate == null)
            {
                return NotFound($"Topic template Id={id} not found");
            }

            var topicTemplateDto = topicTemplate.ToDto<TopicTemplateDto>();

            return Ok(topicTemplateDto);
        }

        /// <summary>
        /// Create topic template
        /// </summary>
        /// <param name="model">Topic template Dto</param>
        [HttpPost]
        [ProducesResponseType(typeof(TopicTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] TopicTemplateDto model)
        {
            var topicTemplate = model.FromDto<TopicTemplate>();

            await _topicTemplateService.InsertTopicTemplateAsync(topicTemplate);

            var topicTemplateDto = topicTemplate.ToDto<TopicTemplateDto>();

            return Ok(topicTemplateDto);
        }

        /// <summary>
        /// Upadete topic by Id
        /// </summary>
        /// <param name="model">Topic Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] TopicTemplateDto model)
        {
            var topicTemplate = await _topicTemplateService.GetTopicTemplateByIdAsync(model.Id);

            if (topicTemplate == null)
                return NotFound("Topic template is not found");

            topicTemplate = model.FromDto<TopicTemplate>();

            await _topicTemplateService.UpdateTopicTemplateAsync(topicTemplate);

            return Ok();
        }

        /// <summary>
        /// Delete topic template
        /// </summary>
        /// <param name="id">Topic template identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var topicTemplate = await _topicTemplateService.GetTopicTemplateByIdAsync(id);

            if (topicTemplate == null)
                return NotFound($"TopicTemplate Id={id} not found");

            await _topicTemplateService.DeleteTopicTemplateAsync(topicTemplate);

            return Ok();
        }

        #endregion
    }
}
