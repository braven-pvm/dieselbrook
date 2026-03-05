
using AnqIntegrationApi.Services.Outbox;
using BrevoApiHelpers.Services;

namespace AnqIntegrationApi.Services.Messaging
{
    public class MessagingDispatcher : IMessagingDispatcher
    {
        private readonly IOutboxPolicy _policy;
        private readonly IOutboxService _outbox;
        private readonly IMessagingService _messaging;
        private readonly ILogger<MessagingDispatcher> _logger;

        public MessagingDispatcher(IOutboxPolicy policy, IOutboxService outbox, IMessagingService messaging, ILogger<MessagingDispatcher> logger)
        {
            _policy = policy;
            _outbox = outbox;
            _messaging = messaging;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, int templateId, Dictionary<string, object> parameters, string? senderName = null, CancellationToken ct = default)
        {

            if (_policy.ShouldQueueEmail(templateId))
            {
                _logger.LogInformation("Queueing email to {To} template {TemplateId}", toEmail, templateId);
                await _outbox.EnqueueEmailAsync(toEmail, templateId, parameters, senderName, ct);
            }
            else
            {
                _logger.LogInformation("Sending email immediately to {To} template {TemplateId}", toEmail, templateId);
                await _messaging.SendTransactionalEmailAsync(toEmail, templateId, parameters);
            }
        }

        public Task SendWhatsAppTemplateAsync(
    string toNumber,
    int templateId,
    Dictionary<string, object> parameters,
    string? senderName = null,
    CancellationToken ct = default)
        {
            return SendWhatsAppTemplateAsync(
                toNumber: toNumber,
                templateId: templateId,
                parameters: parameters,
                brevoContactId: null,
                contactEmail: null,
                senderName: senderName,
                ct: ct);
        }
        public async Task SendWhatsAppTemplateAsync(
        string toNumber,
        int templateId,
        Dictionary<string, object> parameters,
        int? brevoContactId,
        string? contactEmail = null,
        string? senderName = null,
        CancellationToken ct = default)
        {
            if (_policy.ShouldQueueWhatsApp(templateId))
            {
                _logger.LogInformation(
                    "Queueing WhatsApp to {To} template {TemplateId} (BrevoContactID={BrevoContactId}, Email={Email})",
                    toNumber, templateId, brevoContactId, contactEmail);

                await _outbox.EnqueueWhatsAppTemplateAsync(
                    toNumber: toNumber,
                    templateId: templateId,
                    parameters: parameters,
                    senderName: senderName,
                    brevoContactId: brevoContactId,
                    ct: ct);
            }
            else
            {
                _logger.LogInformation(
                    "Sending WhatsApp immediately to {To} template {TemplateId} (BrevoContactID={BrevoContactId}, Email={Email})",
                    toNumber, templateId, brevoContactId, contactEmail);

                await _messaging.SendWhatsappTemplateAsync(toNumber, templateId, parameters);
            }
        }
    }
}
