using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class ForumGroupModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public IList<ForumRowModelDto> Forums { get; set; }
    }
}
