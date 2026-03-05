using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Blog
{
    public partial class BlogPostListModelDto : ModelDto
    {
        public int WorkingLanguageId { get; set; }

        public BlogPagingFilteringModelDto PagingFilteringContext { get; set; }

        public IList<BlogPostModelDto> BlogPosts { get; set; }
    }
}
