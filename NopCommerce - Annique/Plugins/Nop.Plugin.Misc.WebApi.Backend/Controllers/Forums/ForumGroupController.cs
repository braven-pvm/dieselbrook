using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Forums;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Forums;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Forums;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Forums
{
    public partial class ForumGroupController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IForumService _forumService;

        #endregion

        #region Ctor

        public ForumGroupController(IForumService forumService)
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

            var forumGroup = await _forumService.GetForumGroupByIdAsync(id);

            if (forumGroup == null)
                return NotFound($"Forum group Id={id} not found");

            await _forumService.DeleteForumGroupAsync(forumGroup);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ForumGroupDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forumGroup = await _forumService.GetForumGroupByIdAsync(id);

            if (forumGroup == null)
                return NotFound($"Forum group Id={id} not found");

            return Ok(forumGroup.ToDto<ForumGroupDto>());
        }

        /// <summary>
        /// Gets all forum groups
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<ForumGroup, ForumGroupDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var forumGroups = await _forumService.GetAllForumGroupsAsync();
            var forumGroupsDto = forumGroups.Select(fg => fg.ToDto<ForumGroupDto>()).ToList();

            return Ok(forumGroupsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ForumGroupDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ForumGroupDto model)
        {
            var forumGroup = model.FromDto<ForumGroup>();

            await _forumService.InsertForumGroupAsync(forumGroup);

            var forumGroupDto = forumGroup.ToDto<ForumGroupDto>();

            return Ok(forumGroupDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ForumGroupDto model)
        {
            var forumGroup = await _forumService.GetForumGroupByIdAsync(model.Id);

            if (forumGroup == null)
                return NotFound($"Forum group Id={model.Id} is not found");

            forumGroup = model.FromDto<ForumGroup>();

            await _forumService.UpdateForumGroupAsync(forumGroup);

            return Ok();
        }

        #endregion
    }
}
