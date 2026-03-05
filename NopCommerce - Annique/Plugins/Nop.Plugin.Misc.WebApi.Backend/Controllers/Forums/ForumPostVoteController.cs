using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Forums;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Forums;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Forums;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Forums
{
    public partial class ForumPostVoteController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;
        private readonly IRepository<ForumPostVote> _forumPostVoteRepository;

        #endregion

        #region Ctor

        public ForumPostVoteController(ICustomerService customerService,
            IForumService forumService,
            IRepository<ForumPostVote> forumPostVoteRepository)
        {
            _customerService = customerService;
            _forumService = forumService;
            _forumPostVoteRepository = forumPostVoteRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a post vote 
        /// </summary>
        /// <param name="postId">Post identifier</param>
        /// <param name="customerId">Customer identifier</param>
        [HttpGet("{postId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ForumPostVoteDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetPostVote(int postId, int customerId)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var postVote = await _forumService.GetPostVoteAsync(postId, customer);

            return Ok(postVote.ToDto<ForumPostVoteDto>());
        }

        /// <summary>
        /// Get post vote made since the parameter date
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="сreatedFromUtc">Date</param>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNumberOfPostVotes(int customerId, [FromQuery][Required] DateTime сreatedFromUtc)
        {
            if (customerId <= 0)
                return BadRequest();

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            return Ok(await _forumService.GetNumberOfPostVotesAsync(customer, сreatedFromUtc));
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var forumPostVote = await _forumPostVoteRepository.GetByIdAsync(id);

            if (forumPostVote == null)
                return NotFound($"Forum post vote Id={id} not found");

            await _forumService.DeletePostVoteAsync(forumPostVote);

            return Ok();
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ForumPostVoteDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ForumPostVoteDto model)
        {
            var forumPostVote = model.FromDto<ForumPostVote>();

            await _forumService.InsertPostVoteAsync(forumPostVote);

            var forumPostVoteDto = forumPostVote.ToDto<ForumPostVoteDto>();

            return Ok(forumPostVoteDto);
        }

        #endregion
    }
}
