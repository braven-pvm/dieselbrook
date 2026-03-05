
using AnqIntegrationApi.Services.Outbox;

namespace AnqIntegrationApi.Services.Workers
{
    public class BrevoOutboxWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<BrevoOutboxWorker> _logger;
        private static readonly TimeSpan PollDelay = TimeSpan.FromSeconds(5);

        public BrevoOutboxWorker(IServiceProvider sp, ILogger<BrevoOutboxWorker> logger)
        {
            _sp = sp; _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BrevoOutboxWorker started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                    var processed = await processor.ProcessBatchAsync(25, stoppingToken);
                    if (processed == 0)
                        await Task.Delay(PollDelay, stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BrevoOutboxWorker loop error");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            _logger.LogInformation("BrevoOutboxWorker stopping");
        }
    }
}
