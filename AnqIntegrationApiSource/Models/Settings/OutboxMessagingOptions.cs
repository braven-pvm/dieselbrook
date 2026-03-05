
namespace AnqIntegrationApi.Models.Settings
{
    public class OutboxMessagingOptions
    {
        public List<int>? QueuedEmailTemplateIds { get; set; }
        public List<int>? QueuedWhatsAppTemplateIds { get; set; }
    }
}
