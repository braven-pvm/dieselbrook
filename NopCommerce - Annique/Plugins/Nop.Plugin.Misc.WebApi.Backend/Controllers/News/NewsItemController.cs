using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.News;
using Nop.Plugin.Misc.WebApi.Backend.Dto.News;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.News;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.News
{
    public partial class NewsItemController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly INewsService _newsService;

        #endregion

        #region Ctor

        public NewsItemController(INewsService newsService)
        {
            _newsService = newsService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all news items
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="title">Filter by news item title</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<NewsItem, NewsItemDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int languageId = 0, 
            [FromQuery] int storeId = 0,
            [FromQuery] int pageIndex = 0, 
            [FromQuery] int pageSize = int.MaxValue, 
            [FromQuery] bool showHidden = false,
            [FromQuery] string title = null)
        {
            var newsItems = await _newsService.GetAllNewsAsync(languageId, storeId,
                pageIndex, pageSize,
                showHidden, title);

            var pagedListDto = newsItems.ToPagedListDto<NewsItem, NewsItemDto>();

            return Ok(pagedListDto);
        }

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="id">News identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsItemDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var newsItem = await _newsService.GetNewsByIdAsync(id);

            if (newsItem == null)
            {
                return NotFound($"News item Id={id} not found");
            }

            return Ok(newsItem.ToDto<NewsItemDto>());
        }

        /// <summary>
        /// Create a news
        /// </summary>
        /// <param name="model">News item Dto model</param>
        [HttpPost]
        [ProducesResponseType(typeof(NewsItemDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] NewsItemDto model)
        {
            var newsItem = model.FromDto<NewsItem>();

            await _newsService.InsertNewsAsync(newsItem);

            return Ok(newsItem.ToDto<NewsItemDto>());
        }

        /// <summary>
        /// Update a news
        /// </summary>
        /// <param name="model">News item Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] NewsItemDto model)
        {
            var newsItem = await _newsService.GetNewsByIdAsync(model.Id);

            if (newsItem == null)
                return NotFound("News item is not found");

            newsItem = model.FromDto<NewsItem>();
            await _newsService.UpdateNewsAsync(newsItem);

            return Ok();
        }

        /// <summary>
        /// Delete a news
        /// </summary>
        /// <param name="id">News item identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var newsItem = await _newsService.GetNewsByIdAsync(id);

            if (newsItem == null)
                return NotFound($"News item Id={id} not found");

            await _newsService.DeleteNewsAsync(newsItem);

            return Ok();
        }

        /// <summary>
        /// Get a value indicating whether a news item is available now (availability dates)
        /// </summary>
        /// <param name="newsItemId">News item identifier</param>
        /// <param name="dateTime">Datetime to check; pass null to use current date</param>
        [HttpGet("{newsItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> IsNewsAvailable(int newsItemId, [FromQuery] DateTime? dateTime = null)
        {
            if (newsItemId <= 0)
                return BadRequest();

            var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);

            if (newsItem == null) 
                return NotFound($"News item Id={newsItemId} not found");

            return Ok(_newsService.IsNewsAvailable(newsItem, dateTime));
        }

        #endregion
    }
}
