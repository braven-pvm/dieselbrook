
namespace AnqIntegrationApi.Services.Outbox
{
    public interface IOutboxPolicy
    {
        bool ShouldQueueEmail(int templateId);
        bool ShouldQueueWhatsApp(int templateId);
    }
}
