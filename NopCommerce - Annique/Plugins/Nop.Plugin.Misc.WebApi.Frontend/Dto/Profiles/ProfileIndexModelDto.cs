using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Profiles
{
    public partial class ProfileIndexModelDto : ModelDto
    {
        public int CustomerProfileId { get; set; }

        public string ProfileTitle { get; set; }

        public int PostsPage { get; set; }

        public bool PagingPosts { get; set; }

        public bool ForumsEnabled { get; set; }
    }
}
