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
    public partial class ForumPostController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public ForumPostController(IForumService forumService)
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

            var forumPost = await _forumService.GetPostByIdAsync(id);

            if (forumPost == null)
                return NotFound($"Forum post Id={id} not found");

            await _forumService.DeletePostAsync(forumPost);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ForumPostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forumPost = await _forumService.GetPostByIdAsync(id);

            if (forumPost == null)
                return NotFound($"Forum post Id={id} not found");

            return Ok(forumPost.ToDto<ForumPostDto>());
        }

        /// <summary>
        /// Gets all forum posts
        /// </summary>
        /// <param name="forumTopicId">The forum topic identifier</param>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="ascSort">Sort order</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ForumPost, ForumPostDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int forumTopicId = 0,
            [FromQuery] int customerId = 0,
            [FromQuery] string keywords = "",
            [FromQuery] bool ascSort = false,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var forumPosts =
                await _forumService.GetAllPostsAsync(forumTopicId, customerId, keywords, ascSort, pageIndex, pageSize);

            return Ok(forumPosts.ToPagedListDto<ForumPost, ForumPostDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ForumPostDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ForumPostDto model)
        {
            var forumPost = model.FromDto<ForumPost>();

            await _forumService.InsertPostAsync(forumPost, true);

            var forumPostDto = forumPost.ToDto<ForumPostDto>();

            return Ok(forumPostDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ForumPostDto model)
        {
            var forumPost = await _forumService.GetPostByIdAsync(model.Id);

            if (forumPost == null)
                return NotFound($"Forum post Id={model.Id} is not found");

            forumPost = model.FromDto<ForumPost>();

            await _forumService.UpdatePostAsync(forumPost);

            return Ok();
        }

        #endregion
    }
}
