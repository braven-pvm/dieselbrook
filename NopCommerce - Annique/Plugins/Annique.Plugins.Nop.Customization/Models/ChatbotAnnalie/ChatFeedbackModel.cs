using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Annique.Plugins.Nop.Customization.Models.ChatbotAnnalie
{
    public record ChatFeedbackModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.ChatFeedbackModel.Fields.OriginalMessage")]
        public string OriginalMessage { get; set; }

        [NopResourceDisplayName("Admin.ChatFeedbackModel.Fields.AiResponse")]
        public string AiResponse { get; set; }

        [NopResourceDisplayName("Admin.ChatFeedbackModel.Fields.Status")]
        public string Status { get; set; } // "helpful", "not_helpful"

        [NopResourceDisplayName("Admin.ChatFeedbackModel.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.ChatFeedbackModel.Fields.Username")]

        public string Username {  get; set; }

        [NopResourceDisplayName("Admin.ChatFeedbackModel.Fields.IpAddress")]

        public string IpAddress { get; set; }
    }
}
