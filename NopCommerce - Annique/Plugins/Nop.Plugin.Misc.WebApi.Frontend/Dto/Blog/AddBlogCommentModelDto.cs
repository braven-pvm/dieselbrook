using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Blog
{
    public partial class AddBlogCommentModelDto : ModelWithIdDto
    {
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
