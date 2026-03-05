using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class ForumRowModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public string Description { get; set; }

        public int NumTopics { get; set; }

        public int NumPosts { get; set; }

        public int LastPostId { get; set; }
    }
}
