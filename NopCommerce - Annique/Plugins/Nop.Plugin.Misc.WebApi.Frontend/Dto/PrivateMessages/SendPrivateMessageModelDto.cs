using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.PrivateMessages
{
    public partial class SendPrivateMessageModelDto : ModelWithIdDto
    {
        public int ToCustomerId { get; set; }

        public string CustomerToName { get; set; }

        public bool AllowViewingToProfile { get; set; }

        public int ReplyToMessageId { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }
    }
}
