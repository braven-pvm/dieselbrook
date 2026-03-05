using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class CustomerForumSubscriptionsModelDto : ModelDto
    {
        public IList<ForumSubscriptionModelDto> ForumSubscriptions { get; set; }

        public PagerModelDto PagerModel { get; set; }
    }
}
