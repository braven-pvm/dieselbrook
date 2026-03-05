using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class SearchBoardsModelDto : ModelDto
    {
        public bool ShowAdvancedSearch { get; set; }

        public string SearchTerms { get; set; }

        public int? ForumId { get; set; }

        public int? Within { get; set; }

        public int? LimitDays { get; set; }

        public IList<ForumTopicRowModelDto> ForumTopics { get; set; }

        public int TopicPageSize { get; set; }

        public int TopicTotalRecords { get; set; }

        public int TopicPageIndex { get; set; }

        public List<SelectListItemDto> LimitList { get; set; }

        public List<SelectListItemDto> ForumList { get; set; }

        public List<SelectListItemDto> WithinList { get; set; }

        public int ForumIdSelected { get; set; }

        public int WithinSelected { get; set; }

        public int LimitDaysSelected { get; set; }

        public bool SearchResultsVisible { get; set; }

        public bool NoResultsVisisble { get; set; }

        public string Error { get; set; }

        public int PostsPageSize { get; set; }

        public bool AllowPostVoting { get; set; }
    }
}
