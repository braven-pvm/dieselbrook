using AnqIntegrationApi.Models.Settings;
using Microsoft.Extensions.Options;

namespace AnqIntegrationApi.Services.Outbox
{
    public class OutboxPolicy : IOutboxPolicy
    {
        private readonly OutboxMessagingOptions _opts;

        public OutboxPolicy(IOptions<OutboxMessagingOptions> opts)
        {
            _opts = opts.Value ?? new OutboxMessagingOptions();
        }

        public bool ShouldQueueEmail(int templateId) =>
            _opts.QueuedEmailTemplateIds?.Contains(templateId) == true;

        public bool ShouldQueueWhatsApp(int templateId) =>
            _opts.QueuedWhatsAppTemplateIds?.Contains(templateId) == true;
    }
}
