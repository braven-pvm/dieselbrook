using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class BoardsIndexModelDto : ModelDto
    {
        public IList<ForumGroupModelDto> ForumGroups { get; set; }
    }
}
