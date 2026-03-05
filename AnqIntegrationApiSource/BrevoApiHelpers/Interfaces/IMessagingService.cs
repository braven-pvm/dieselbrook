using BrevoApiHelpers.Models;

public interface IMessagingService
{
    Task<BrevoEmailResponse> SendTransactionalEmailAsync(
        string toEmail,
        int templateId,
        Dictionary<string, object> parameters);

    Task<BrevoEmailResponse> SendWhatsappTemplateAsync(
        string toNumber,
        int templateId,
        Dictionary<string, object> parameters);
}
