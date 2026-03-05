using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.News
{
    public partial class AddNewsCommentModelDto : ModelDto
    {
        public string CommentTitle { get; set; }

        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
