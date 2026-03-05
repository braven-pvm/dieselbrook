using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class PostVoteResponse : BaseDto
    {
        public string Error { get; set; }

        public int VoteCount { get; set; }

        public bool IsUp { get; set; }
    }
}
