using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.PrivateMessages
{
    public partial class PrivateMessageIndexModelDto : ModelDto
    {
        public int InboxPage { get; set; }

        public int SentItemsPage { get; set; }

        public bool SentItemsTabSelected { get; set; }
    }
}
