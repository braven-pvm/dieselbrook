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
    public partial class ForumSubscriptionController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public ForumSubscriptionController(IForumService forumService)
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

            var forumSubscription = await _forumService.GetSubscriptionByIdAsync(id);

            if (forumSubscription == null)
                return NotFound($"Forum subscription Id={id} not found");

            await _forumService.DeleteSubscriptionAsync(forumSubscription);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ForumSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forumSubscription = await _forumService.GetSubscriptionByIdAsync(id);

            if (forumSubscription == null)
                return NotFound($"Forum subscription Id={id} not found");

            return Ok(forumSubscription.ToDto<ForumSubscriptionDto>());
        }

        /// <summary>
        /// Gets forum subscriptions
        /// </summary>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="topicId">The topic identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ForumSubscription, ForumSubscriptionDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int customerId = 0,
            [FromQuery] int forumId = 0,
            [FromQuery] int topicId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var forumSubscriptions =
                await _forumService.GetAllSubscriptionsAsync(customerId, forumId, topicId, pageIndex, pageSize);

            return Ok(forumSubscriptions.ToPagedListDto<ForumSubscription, ForumSubscriptionDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ForumSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ForumSubscriptionDto model)
        {
            var forumSubscription = model.FromDto<ForumSubscription>();

            await _forumService.InsertSubscriptionAsync(forumSubscription);

            var forumSubscriptionDto = forumSubscription.ToDto<ForumSubscriptionDto>();

            return Ok(forumSubscriptionDto);
        }

        #endregion
    }
}
