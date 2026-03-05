using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.News;
using Nop.Plugin.Misc.WebApi.Backend.Dto.News;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.News;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.News
{
    public partial class NewsCommentController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly INewsService _newsService;

        #endregion

        #region Ctor

        public NewsCommentController(INewsService newsService)
        {
            _newsService = newsService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all news comment
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="newsItemId">News item ID; 0 or null to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item creation to; null to load all records</param>
        /// <param name="commentText">Search comment text; null to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<NewsCommentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int customerId = 0,
            [FromQuery] int storeId = 0,
            [FromQuery] int? newsItemId = null,
            [FromQuery] bool? approved = null,
            [FromQuery] DateTime? fromUtc = null,
            [FromQuery] DateTime? toUtc = null,
            [FromQuery] string commentText = null)
        {
            var newsComments = await _newsService.GetAllCommentsAsync(customerId, storeId, newsItemId,
                approved, fromUtc, toUtc, commentText);

            var newsCommentDtos = newsComments.Select(item => item.ToDto<NewsCommentDto>()).ToList();

            return Ok(newsCommentDtos);
        }

        /// <summary>
        /// Gets a news comment
        /// </summary>
        /// <param name="id">News comment identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsCommentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var newsComment = await _newsService.GetNewsCommentByIdAsync(id);

            if (newsComment == null)
            {
                return NotFound($"News comment Id={id} not found");
            }

            return Ok(newsComment.ToDto<NewsCommentDto>());
        }

        /// <summary>
        /// Get news comments by identifiers
        /// </summary>
        /// <param name="ids">Array of news comment identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<NewsCommentDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByIds(string ids)
        {
            var newsCommentIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var newsComments = await _newsService.GetNewsCommentsByIdsAsync(newsCommentIds);

            var newsCommentDtos = newsComments.Select(newsComment => newsComment.ToDto<NewsCommentDto>()).ToList();

            return Ok(newsCommentDtos);
        }

        /// <summary>
        /// Create a news comment
        /// </summary>
        /// <param name="model">News comment Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(NewsCommentDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] NewsCommentDto model)
        {
            var newsComment = model.FromDto<NewsComment>();

            await _newsService.InsertNewsCommentAsync(newsComment);

            return Ok(newsComment.ToDto<NewsCommentDto>());
        }

        /// <summary>
        /// Update a news comment
        /// </summary>
        /// <param name="model">News comment Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] NewsCommentDto model)
        {
            var newsComment = await _newsService.GetNewsCommentByIdAsync(model.Id);

            if (newsComment == null)
                return NotFound("News comment is not found");

            newsComment = model.FromDto<NewsComment>();
            await _newsService.UpdateNewsCommentAsync(newsComment);

            return Ok();
        }

        /// <summary>
        /// Delete a news comment
        /// </summary>
        /// <param name="id">News comment identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var newsComment = await _newsService.GetNewsCommentByIdAsync(id);

            if (newsComment == null)
                return NotFound($"News comment Id={id} not found");

            await _newsService.DeleteNewsCommentAsync(newsComment);

            return Ok();
        }

        /// <summary>
        /// Deletes a news comments by identifiers
        /// </summary>
        /// <param name="ids">Array of news comment identifiers (separator - ;)</param>
        [HttpDelete("{ids}")]
        public virtual async Task<IActionResult> DeleteByIds(string ids)
        {
            var newsCommentIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var newsComments = await _newsService.GetNewsCommentsByIdsAsync(newsCommentIds);

            await _newsService.DeleteNewsCommentsAsync(newsComments);

            return Ok();
        }

        /// <summary>
        /// Get the count of news comments
        /// </summary>
        /// <param name="newsItemId">News item identifier</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="isApproved">A value indicating whether to count only approved or not approved comments; pass null to get number of all comments</param>
        [HttpGet("{newsItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNewsCommentsCount(int newsItemId,
            [FromQuery] int storeId = 0,
            [FromQuery] bool? isApproved = null)
        {
            if (newsItemId <= 0)
                return BadRequest();

            var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);

            if (newsItem == null) 
                return NotFound($"News item Id={newsItemId} not found");

            return Ok(await _newsService.GetNewsCommentsCountAsync(newsItem, storeId, isApproved));
        }

        #endregion
    }
}
