
namespace AnqIntegrationApi.Services.Messaging
{
    public interface IMessagingDispatcher
    {
        Task SendEmailAsync(string toEmail, int templateId, Dictionary<string, object> parameters, string? senderName = null, CancellationToken ct = default);
        Task SendWhatsAppTemplateAsync(
        string toNumber,
        int templateId,
        Dictionary<string, object> parameters,
        string? senderName = null,
        CancellationToken ct = default);

        Task SendWhatsAppTemplateAsync(
            string toNumber,
            int templateId,
            Dictionary<string, object> parameters,
            int? brevoContactId,
            string? contactEmail = null,
            string? senderName = null,
            CancellationToken ct = default);
    }
}
