using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie
{
    public class ChatbotFeedback : BaseEntity
    {
        public string OriginalMessage { get; set; }
        public string AiResponse { get; set; }
        public string Status { get; set; } // "helpful", "not_helpful"

        public string IpAddress { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public string Username { get; set; }
    }
}
