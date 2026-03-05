using Nop.Core.Domain.Forums;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards
{
    public partial class ForumTopicRowModelDto : ModelWithIdDto
    {
        public string Subject { get; set; }

        public string SeName { get; set; }

        public int LastPostId { get; set; }

        public int NumPosts { get; set; }

        public int Views { get; set; }

        public int Votes { get; set; }

        public int NumReplies { get; set; }

        public ForumTopicType ForumTopicType { get; set; }

        public int CustomerId { get; set; }

        public bool AllowViewingProfiles { get; set; }

        public string CustomerName { get; set; }

        //posts
        public int TotalPostPages { get; set; }
    }
}
