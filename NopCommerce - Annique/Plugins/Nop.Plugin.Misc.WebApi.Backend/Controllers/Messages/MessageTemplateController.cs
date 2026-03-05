using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Messages;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Messages
{
    public partial class MessageTemplateController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;

        #endregion

        #region Ctor

        public MessageTemplateController(IMessageTemplateService messageTemplateService)
        {
            _messageTemplateService = messageTemplateService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a message template
        /// </summary>
        /// <param name="id">Message template identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(id);

            if (messageTemplate == null)
                return NotFound($"Message template Id={id} not found");

            await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);

            return Ok();
        }

        /// <summary>
        /// Create a message template
        /// </summary>
        /// <param name="model">Message template Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(MessageTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] MessageTemplateDto model)
        {
            var messageTemplate = model.FromDto<MessageTemplate>();

            await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);

            return Ok(messageTemplate.ToDto<MessageTemplateDto>());
        }

        /// <summary>
        /// Update a message template
        /// </summary>
        /// <param name="model">Message template Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] MessageTemplateDto model)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(model.Id);

            if (messageTemplate == null)
                return NotFound("Message template is not found");

            messageTemplate = model.FromDto<MessageTemplate>();
            await _messageTemplateService.UpdateMessageTemplateAsync(messageTemplate);

            return Ok();
        }

        /// <summary>
        /// Gets a message template by identifier
        /// </summary>
        /// <param name="id">The message template identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(id);

            if (messageTemplate == null)
            {
                return NotFound($"Message template Id={id} not found");
            }

            return Ok(messageTemplate.ToDto<MessageTemplateDto>());
        }

        /// <summary>
        /// Gets a message templates by name
        /// </summary>
        /// <param name="name">The message template name</param>
        /// <param name="storeId">Store identifier</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<MessageTemplateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByName([FromQuery][Required] string name, [FromQuery] int? storeId = null)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(name, storeId);

            if (messageTemplates == null)
            {
                return NotFound($"Message templates is not found");
            }

            var messageTemplateDtos = messageTemplates.Select(messageTemplate => messageTemplate.ToDto<MessageTemplateDto>()).ToList();

            return Ok(messageTemplateDtos);
        }

        /// <summary>
        /// Gets all message templates
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="keywords">Keywords to search body or subject</param>
        /// <param name="isActive">A value indicating whether to get active records; "null" to load all records; "false" to load only inactive records; "true" to load only active records</param>
        [HttpGet("{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<MessageTemplateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int storeId, [FromQuery] string keywords = null, bool? isActive = null)
        {
            if (storeId < 0)
                return BadRequest();

            var messageTemplates = await _messageTemplateService.GetAllMessageTemplatesAsync(storeId, keywords, isActive);
            var messageTemplateDtos = messageTemplates.Select(messageTemplate => messageTemplate.ToDto<MessageTemplateDto>()).ToList();

            return Ok(messageTemplateDtos);
        }

        /// <summary>
        /// Create a copy of message template with all depended data
        /// </summary>
        /// <param name="model">Message template Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(MessageTemplateDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Copy([FromBody] MessageTemplateDto model)
        {
            var messageTemplate = model.FromDto<MessageTemplate>();

            var copyMessageTemplate = await _messageTemplateService.CopyMessageTemplateAsync(messageTemplate);

            return Ok(copyMessageTemplate.ToDto<MessageTemplateDto>());
        }

        #endregion
    }
}
