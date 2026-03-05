using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Topics;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class TopicController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITopicModelFactory _topicModelFactory;
        private readonly ITopicService _topicService;

        #endregion

        #region Ctor

        public TopicController(IAclService aclService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            ITopicModelFactory topicModelFactory,
            ITopicService topicService)
        {
            _aclService = aclService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _topicModelFactory = topicModelFactory;
            _topicService = topicService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a topic details
        /// </summary>
        /// <param name="id">The topic identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTopicDetails(int id)
        {
            if (id <= 0)
                return BadRequest();

            var topic = await _topicService.GetTopicByIdAsync(id);

            if(topic == null)
                return NotFound($"Topic Id={id} not found");

            var notAvailable = !topic.Published ||
                               //ACL (access control list)
                               !await _aclService.AuthorizeAsync(topic) ||
                               //store mapping
                               !await _storeMappingService.AuthorizeAsync(topic);

            //allow administrators to preview any topic
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTopics);

            if (notAvailable && !hasAdminAccess)
                return NotFound($"Topic Id={id} not found");

            var topicModel = await _topicModelFactory.PrepareTopicModelAsync(topic);

            var topicDto = topicModel.ToDto<TopicModelDto>();

            return Ok(topicDto);
        }

        /// <summary>
        /// Gets a topic details by system name
        /// </summary>
        /// <param name="systemName">The topic identifier</param>
        [HttpGet("{systemName}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTopicDetailsBySystemName(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return BadRequest();

            var topicModel = await _topicModelFactory.PrepareTopicModelBySystemNameAsync(systemName);
            if (topicModel == null)
                return NotFound($"Topic systemName={systemName} not found");

            var topicDto = topicModel.ToDto<TopicModelDto>();

            return Ok(topicDto);
        }

        #endregion
    }
}
