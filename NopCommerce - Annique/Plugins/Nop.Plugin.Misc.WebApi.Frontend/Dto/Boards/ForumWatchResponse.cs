using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class ForumWatchResponse : BaseDto
    {
        public bool Subscribed { get; set; }

        public string Text { get; set; }

        public bool Error { get; set; }
    }
}
