using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.News
{
    public partial class NewsItemModelDto : ModelWithIdDto
    {
        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public string Title { get; set; }

        public string Short { get; set; }

        public string Full { get; set; }

        public bool AllowComments { get; set; }

        public bool PreventNotRegisteredUsersToLeaveComments { get; set; }

        public int NumberOfComments { get; set; }

        public DateTime CreatedOn { get; set; }

        public IList<NewsCommentModelDto> Comments { get; set; }

        public AddNewsCommentModelDto AddNewComment { get; set; }
    }
}
