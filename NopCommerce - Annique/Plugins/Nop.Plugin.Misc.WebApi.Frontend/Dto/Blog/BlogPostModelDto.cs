using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Blog
{
    public partial class BlogPostModelDto : ModelWithIdDto
    {
        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string BodyOverview { get; set; }

        public bool AllowComments { get; set; }

        public bool PreventNotRegisteredUsersToLeaveComments { get; set; }

        public int NumberOfComments { get; set; }

        public DateTime CreatedOn { get; set; }

        public IList<string> Tags { get; set; }

        public IList<BlogCommentModelDto> Comments { get; set; }

        public AddBlogCommentModelDto AddNewComment { get; set; }
    }
}
