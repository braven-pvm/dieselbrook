
namespace AnqIntegrationApi.Services.Outbox
{
    public interface IOutboxService
    {
        Task<long> EnqueueEmailAsync(string toEmail, int templateId, Dictionary<string, object> parameters, string? senderName = null, CancellationToken ct = default);
        Task<long> EnqueueWhatsAppTemplateAsync(
                string toNumber,
                int templateId,
                Dictionary<string, object> parameters,
                string? senderName = null,
                CancellationToken ct = default);

       Task<long> EnqueueWhatsAppTemplateAsync(
                string toNumber,
                int templateId,
                Dictionary<string, object> parameters,
                string? senderName,
                int? brevoContactId,
                CancellationToken ct = default);
        }
    }

