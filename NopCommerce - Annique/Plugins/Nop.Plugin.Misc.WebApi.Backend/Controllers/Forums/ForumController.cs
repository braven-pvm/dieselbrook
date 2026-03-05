using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Forums;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Forums;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Forums;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Forums
{
    public partial class ForumController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public ForumController(ICustomerService customerService,
            IForumService forumService)
        {
            _customerService = customerService;
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

            var forum = await _forumService.GetForumByIdAsync(id);

            if (forum == null)
                return NotFound($"Forum Id={id} not found");

            await _forumService.DeleteForumAsync(forum);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ForumDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forum = await _forumService.GetForumByIdAsync(id);

            if (forum == null)
                return NotFound($"Forum Id={id} not found");

            return Ok(forum.ToDto<ForumDto>());
        }

        /// <summary>
        /// Gets forums by group identifier
        /// </summary>
        /// <param name="forumGroupId">The forum group identifier</param>
        [HttpGet("{forumGroupId}")]
        [ProducesResponseType(typeof(IList<ForumDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int forumGroupId)
        {
            var forums = await _forumService.GetAllForumsByGroupIdAsync(forumGroupId);
            var forumsDto = forums.Select(f => f.ToDto<ForumDto>()).ToList();

            return Ok(forumsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ForumDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ForumDto model)
        {
            var forum = model.FromDto<Forum>();

            await _forumService.InsertForumAsync(forum);

            var forumDto = forum.ToDto<ForumDto>();

            return Ok(forumDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ForumDto model)
        {
            var forum = await _forumService.GetForumByIdAsync(model.Id);

            if (forum == null)
                return NotFound($"Forum Id={model.Id} is not found");

            forum = model.FromDto<Forum>();

            await _forumService.UpdateForumAsync(forum);

            return Ok();
        }

        /// <summary>
        /// Check whether customer is allowed to create new topics
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="forumId">Forum Id</param>
        [HttpGet("{customerId}/{forumId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToCreateTopic(int customerId, int forumId)
        {
            if (customerId <= 0 || forumId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var forum = await _forumService.GetForumByIdAsync(forumId);

            if (forum == null)
                return NotFound($"Forum Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToCreateTopicAsync(customer, forum));
        }

        /// <summary>
        /// Check whether customer is allowed to edit topic
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="topicId">Topic Id</param>
        [HttpGet("{customerId}/{topicId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToEditTopic(int customerId, int topicId)
        {
            if (customerId <= 0 || topicId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var topic = await _forumService.GetTopicByIdAsync(topicId);

            if (topic == null)
                return NotFound($"Forum topic Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToEditTopicAsync(customer, topic));
        }

        /// <summary>
        /// Check whether customer is allowed to move topic
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="topicId">Topic Id</param>
        [HttpGet("{customerId}/{topicId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToMoveTopic(int customerId, int topicId)
        {
            if (customerId <= 0 || topicId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var topic = await _forumService.GetTopicByIdAsync(topicId);

            if (topic == null)
                return NotFound($"Forum topic Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToMoveTopicAsync(customer, topic));
        }

        /// <summary>
        /// Check whether customer is allowed to delete topic
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="topicId">Topic Id</param>
        [HttpGet("{customerId}/{topicId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToDeleteTopic(int customerId, int topicId)
        {
            if (customerId <= 0 || topicId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var topic = await _forumService.GetTopicByIdAsync(topicId);

            if (topic == null)
                return NotFound($"Forum topic Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToDeleteTopicAsync(customer, topic));
        }

        /// <summary>
        /// Check whether customer is allowed to create new post
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="topicId">Topic Id</param>
        [HttpGet("{customerId}/{topicId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToCreatePost(int customerId, int topicId)
        {
            if (customerId <= 0 || topicId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var topic = await _forumService.GetTopicByIdAsync(topicId);

            if (topic == null)
                return NotFound($"Forum topic Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToCreatePostAsync(customer, topic));
        }

        /// <summary>
        /// Check whether customer is allowed to edit post
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="postId">Topic post Id</param>
        [HttpGet("{customerId}/{postId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToEditPost(int customerId, int postId)
        {
            if (customerId <= 0 || postId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var post = await _forumService.GetPostByIdAsync(postId);

            if (post == null)
                return NotFound($"Forum topic post Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToEditPostAsync(customer, post));
        }

        /// <summary>
        /// Check whether customer is allowed to delete post
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <param name="postId">Topic post Id</param>
        [HttpGet("{customerId}/{postId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToDeletePost(int customerId, int postId)
        {
            if (customerId <= 0)
                return BadRequest();

            if (postId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var post = await _forumService.GetPostByIdAsync(postId);

            if (post == null)
                return NotFound($"Forum topic post Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToDeletePostAsync(customer, post));
        }

        /// <summary>
        /// Check whether customer is allowed to set topic priority
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToSetTopicPriority(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToSetTopicPriorityAsync(customer));
        }

        /// <summary>
        /// Check whether customer is allowed to watch topics
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsCustomerAllowedToSubscribe(int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _forumService.IsCustomerAllowedToSubscribeAsync(customer));
        }

        #endregion
    }
}
