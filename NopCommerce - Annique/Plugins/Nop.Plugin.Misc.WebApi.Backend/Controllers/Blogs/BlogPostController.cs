using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Blogs;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Blogs;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Blogs;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Blogs
{
    public partial class BlogPostController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IBlogService _blogService;

        #endregion

        #region Ctor

        public BlogPostController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a blog post
        /// </summary>
        /// <param name="id">Blog post Id</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var blogPost = await _blogService.GetBlogPostByIdAsync(id);

            if (blogPost == null)
                return NotFound($"Blog post Id={id} not found");

            await _blogService.DeleteBlogPostAsync(blogPost);

            return Ok();
        }

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="id">Blog post identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var blogPost = await _blogService.GetBlogPostByIdAsync(id);

            if (blogPost == null)
                return NotFound($"Blog post Id={id} not found");

            return Ok(blogPost.ToDto<BlogPostDto>());
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="title">Filter by blog post title</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<BlogPost, BlogPostDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int storeId = 0,
            [FromQuery] int languageId = 0,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false,
            [FromQuery] string title = null)
        {
            var blogPosts = await _blogService.GetAllBlogPostsAsync(storeId, languageId, dateFrom, dateTo,
                pageIndex, pageSize, showHidden, title);

            return Ok(blogPosts.ToPagedListDto<BlogPost, BlogPostDto>());
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all blog posts</param>
        /// <param name="tag">Tag</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<BlogPost, BlogPostDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllByTag([FromQuery] int storeId = 0,
            [FromQuery] int languageId = 0,
            [FromQuery] string tag = "",
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var blogPosts = await _blogService.GetAllBlogPostsByTagAsync(storeId, languageId, tag,
                pageIndex, pageSize, showHidden);

            return Ok(blogPosts.ToPagedListDto<BlogPost, BlogPostDto>());
        }

        /// <summary>
        /// Inserts a blog post
        /// </summary>
        /// <param name="model">Blog post Dto</param>
        [HttpPost]
        [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] BlogPostDto model)
        {
            var blogPost = model.FromDto<BlogPost>();

            await _blogService.InsertBlogPostAsync(blogPost);

            var blogPostDto = blogPost.ToDto<BlogPostDto>();

            return Ok(blogPostDto);
        }

        /// <summary>
        /// Updates the blog post
        /// </summary>
        /// <param name="model">Blog post Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] BlogPostDto model)
        {
            var blogPost = await _blogService.GetBlogPostByIdAsync(model.Id);

            if (blogPost == null)
                return NotFound($"Blog post Id={model.Id} is not found");

            blogPost = model.FromDto<BlogPost>();

            await _blogService.UpdateBlogPostAsync(blogPost);

            return Ok();
        }

        /// <summary>
        /// Parse tags
        /// </summary>
        /// <param name="id">Blog post id</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ParseTags(int id)
        {
            if (id <= 0)
                return BadRequest();

            var blogPost = await _blogService.GetBlogPostByIdAsync(id);

            if (blogPost == null)
                return NotFound($"Blog post Id={id} not found");

            return Ok(await _blogService.ParseTagsAsync(blogPost));
        }

        /// <summary>
        /// Get a value indicating whether a blog post is available now (availability dates)
        /// </summary>
        /// <param name="id">Blog post id</param>
        /// <param name="dateTime">Datetime to check; pass null to use current date</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> BlogPostIsAvailable(int id, [FromQuery] DateTime? dateTime = null)
        {
            if (id <= 0)
                return BadRequest();

            var blogPost = await _blogService.GetBlogPostByIdAsync(id);

            if (blogPost == null)
                return NotFound($"Blog post Id={id} not found");

            return Ok(_blogService.BlogPostIsAvailable(blogPost, dateTime));
        }

        #endregion
    }
}
