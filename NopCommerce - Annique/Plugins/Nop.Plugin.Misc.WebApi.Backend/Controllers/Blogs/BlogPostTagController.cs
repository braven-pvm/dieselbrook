using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Blogs;
using Nop.Services.Blogs;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Blogs
{
    public partial class BlogPostTagController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IBlogService _blogService;

        #endregion

        #region Ctor

        public BlogPostTagController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all blog post tags
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all blog posts</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet("{storeId}/{languageId}")]
        public virtual async Task<IActionResult> GetAll(int storeId, int languageId, [FromQuery] bool showHidden = false)
        {
            var blogPostTags = await _blogService.GetAllBlogPostTagsAsync(storeId, languageId, showHidden);

            var blogPostTagsDto = blogPostTags.Select(tag => new BlogPostTagDto
            {
                BlogPostCount = tag.BlogPostCount,
                Name = tag.Name
            }).ToList();

            return Ok(blogPostTagsDto);
        }
    }

    #endregion
}
