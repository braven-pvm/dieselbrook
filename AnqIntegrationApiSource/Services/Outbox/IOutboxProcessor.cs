
namespace AnqIntegrationApi.Services.Outbox
{
    public interface IOutboxProcessor
    {
        Task<int> ProcessBatchAsync(int maxBatchSize, CancellationToken ct = default);
    }
}
