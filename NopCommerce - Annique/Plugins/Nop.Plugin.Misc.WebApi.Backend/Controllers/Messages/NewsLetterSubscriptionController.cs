using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Messages;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Messages
{
    public partial class NewsLetterSubscriptionController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionController(INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a newsletter subscription
        /// </summary>
        /// <param name="model">NewsLetter subscription Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(NewsLetterSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] NewsLetterSubscriptionDto model)
        {
            var newsLetterSubscription = model.FromDto<NewsLetterSubscription>();

            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(newsLetterSubscription);

            return Ok(newsLetterSubscription.ToDto<NewsLetterSubscriptionDto>());
        }

        /// <summary>
        /// Update a newsletter subscription
        /// </summary>
        /// <param name="model">NewsLetter subscription Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] NewsLetterSubscriptionDto model)
        {
            var newsLetterSubscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(model.Id);

            if (newsLetterSubscription == null)
                return NotFound("NewsLetter subscription is not found");

            newsLetterSubscription = model.FromDto<NewsLetterSubscription>();
            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsLetterSubscription);

            return Ok();
        }

        /// <summary>
        /// Delete a newsLetter subscription
        /// </summary>
        /// <param name="id">NewsLetter subscription identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var newsLetterSubscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(id);

            if (newsLetterSubscription == null)
                return NotFound($"NewsLetter subscription Id={id} not found");

            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsLetterSubscription);

            return Ok();
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="id">The newsletter subscription identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsLetterSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var newsLetterSubscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(id);

            if (newsLetterSubscription == null)
            {
                return NotFound($"NewsLetter subscription Id={id} not found");
            }

            return Ok(newsLetterSubscription.ToDto<NewsLetterSubscriptionDto>());
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="guid">The newsletter subscription GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NewsLetterSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByGuid([FromQuery][Required] Guid guid)
        {
            var newsLetterSubscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuidAsync(guid);

            if (newsLetterSubscription == null)
            {
                return NotFound("NewsLetter subscription is not found");
            }

            return Ok(newsLetterSubscription.ToDto<NewsLetterSubscriptionDto>());
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
        /// <param name="storeId">Store identifier</param>
        [HttpGet("{storeId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NewsLetterSubscriptionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByEmailAndStoreId([FromQuery][Required] string email, int storeId)
        {
            var newsLetterSubscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(email, storeId);

            if (newsLetterSubscription == null)
                return NotFound("NewsLetter subscription not found");

            return Ok(newsLetterSubscription.ToDto<NewsLetterSubscriptionDto>());
        }

        /// <summary>
        /// Gets the newsletter subscription list
        /// </summary>
        /// <param name="email">Email to search or string. Empty to load all records.</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="storeId">Store identifier. 0 to load all records.</param>
        /// <param name="customerRoleId">Customer role identifier. Used to filter subscribers by customer role. 0 to load all records.</param>
        /// <param name="isActive">Value indicating whether subscriber record should be active or not; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<NewsLetterSubscription, NewsLetterSubscriptionDto>),
            StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string email = null,
            [FromQuery] DateTime? createdFromUtc = null,
            [FromQuery] DateTime? createdToUtc = null,
            [FromQuery] int storeId = 0,
            [FromQuery] bool? isActive = null,
            [FromQuery] int customerRoleId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var newsLetterSubscriptions = await _newsLetterSubscriptionService.GetAllNewsLetterSubscriptionsAsync(email,
                createdFromUtc, createdToUtc,
                storeId, isActive, customerRoleId,
                pageIndex, pageSize);

            var pagedListDto =
                newsLetterSubscriptions.ToPagedListDto<NewsLetterSubscription, NewsLetterSubscriptionDto>();

            return Ok(pagedListDto);
        }

        #endregion
    }
}
