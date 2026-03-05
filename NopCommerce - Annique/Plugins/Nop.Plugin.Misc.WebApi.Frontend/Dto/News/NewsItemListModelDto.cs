using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Web.Models.News;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.News
{
    public partial class NewsItemListModelDto : ModelDto
    {
        public int WorkingLanguageId { get; set; }

        public NewsPagingFilteringModel PagingFilteringContext { get; set; }

        public IList<NewsItemModelDto> NewsItems { get; set; }
    }
}
