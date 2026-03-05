using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class TopicMoveModelDto : ModelWithIdDto
    {
        public int ForumSelected { get; set; }

        public string TopicSeName { get; set; }

        public IEnumerable<SelectListItemDto> ForumList { get; set; }
    }
}
