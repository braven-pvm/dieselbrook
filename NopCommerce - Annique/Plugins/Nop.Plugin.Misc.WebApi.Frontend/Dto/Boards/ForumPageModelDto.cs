using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class ForumPageModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public string Description { get; set; }

        public string WatchForumText { get; set; }

        public IList<ForumTopicRowModelDto> ForumTopics { get; set; }

        public int TopicPageSize { get; set; }

        public int TopicTotalRecords { get; set; }

        public int TopicPageIndex { get; set; }

        public bool IsCustomerAllowedToSubscribe { get; set; }

        public bool ForumFeedsEnabled { get; set; }

        public int PostsPageSize { get; set; }

        public bool AllowPostVoting { get; set; }
    }
}
