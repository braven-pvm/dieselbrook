using System.ComponentModel.DataAnnotations;
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
    public partial class PrivateMessageController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public PrivateMessageController(IForumService forumService)
        {
            _forumService = forumService;
        }

        #endregion

        #region Methods
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var privateMessage = await _forumService.GetPrivateMessageByIdAsync(id);

            if (privateMessage == null)
                return NotFound($"Private message Id={id} not found");

            await _forumService.DeletePrivateMessageAsync(privateMessage);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PrivateMessageDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var privateMessage = await _forumService.GetPrivateMessageByIdAsync(id);

            if (privateMessage == null)
                return NotFound($"Private message Id={id} not found");

            return Ok(privateMessage.ToDto<PrivateMessageDto>());
        }

        /// <summary>
        /// Gets private messages
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all messages</param>
        /// <param name="fromCustomerId">The customer identifier who sent the message</param>
        /// <param name="toCustomerId">The customer identifier who should receive the message</param>
        /// <param name="isRead">A value indicating whether loaded messages are read. false - to load not read messages only, 1 to load read messages only, null to load all messages</param>
        /// <param name="isDeletedByAuthor">A value indicating whether loaded messages are deleted by author. false - messages are not deleted by author, null to load all messages</param>
        /// <param name="isDeletedByRecipient">A value indicating whether loaded messages are deleted by recipient. false - messages are not deleted by recipient, null to load all messages</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{storeId}/{fromCustomerId}/{toCustomerId}")]
        [ProducesResponseType(typeof(PagedListDto<PrivateMessage, PrivateMessageDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int storeId,
            int fromCustomerId,
            int toCustomerId,
            [FromQuery] bool? isRead,
            [FromQuery] bool? isDeletedByAuthor,
            [FromQuery] bool? isDeletedByRecipient,
            [FromQuery, Required] string keywords,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var privateMessages = await _forumService.GetAllPrivateMessagesAsync(storeId, fromCustomerId, toCustomerId,
                isRead,
                isDeletedByAuthor, isDeletedByRecipient, keywords, pageIndex, pageSize);

            return Ok(privateMessages.ToPagedListDto<PrivateMessage, PrivateMessageDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(PrivateMessageDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] PrivateMessageDto model)
        {
            var privateMessage = model.FromDto<PrivateMessage>();

            await _forumService.InsertPrivateMessageAsync(privateMessage);

            var privateMessageDto = privateMessage.ToDto<PrivateMessageDto>();

            return Ok(privateMessageDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] PrivateMessageDto model)
        {
            var privateMessage = await _forumService.GetPrivateMessageByIdAsync(model.Id);

            if (privateMessage == null)
                return NotFound($"Private message Id={model.Id} is not found");

            privateMessage = model.FromDto<PrivateMessage>();

            await _forumService.UpdatePrivateMessageAsync(privateMessage);

            return Ok();
        }

        #endregion
    }
}
