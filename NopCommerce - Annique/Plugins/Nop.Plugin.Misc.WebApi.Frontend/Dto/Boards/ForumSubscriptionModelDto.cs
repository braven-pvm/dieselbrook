using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class ForumSubscriptionModelDto : ModelWithIdDto
    {
        public int ForumId { get; set; }

        public int ForumTopicId { get; set; }

        public bool TopicSubscription { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }
    }
}
