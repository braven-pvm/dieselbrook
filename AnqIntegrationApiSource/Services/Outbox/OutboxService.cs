using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AnqIntegrationApi.Services.Outbox
{
    public class OutboxService : IOutboxService
    {
        private readonly OutboxDbContext _db;
        private readonly ILogger<OutboxService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public OutboxService(OutboxDbContext db, ILogger<OutboxService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<long> EnqueueEmailAsync(
            string toEmail,
            int templateId,
            Dictionary<string, object> parameters,
            string? senderName = null,
            CancellationToken ct = default)
        {
            var c = _db.Database.GetDbConnection();
            _logger.LogInformation("OUTBOXSERVICE csLen={Len} DS='{DS}' DB='{DB}'",
                c.ConnectionString?.Length ?? 0, c.DataSource, c.Database);

            _logger.LogInformation(
                "OUTBOX EMAIL ENQUEUE: To={To}, TemplateId={TemplateId}, Sender={Sender}, Params={Params}",
                toEmail,
                templateId,
                senderName ?? "(none)",
                JsonSerializer.Serialize(parameters ?? new(), _jsonOpts));

            var msg = new BrevoOutboxMessage
            {
                Type = BrevoOutboxType.Email,
                To = toEmail,
                TemplateId = templateId,
                ParamsJson = JsonSerializer.Serialize(parameters ?? new(), _jsonOpts),
                SenderName = senderName,
                Status = BrevoOutboxStatus.Pending,
                NextAttemptUtc = DateTimeOffset.UtcNow
            };

            _db.BrevoOutbox.Add(msg);
            await _db.SaveChangesAsync(ct);

            return msg.Id;
        }

        // Keep existing method exactly as-is, but forward to new overload
        public Task<long> EnqueueWhatsAppTemplateAsync(
            string toNumber,
            int templateId,
            Dictionary<string, object> parameters,
            string? senderName = null,
            CancellationToken ct = default)
        {
            return EnqueueWhatsAppTemplateAsync(
                toNumber: toNumber,
                templateId: templateId,
                parameters: parameters,
                senderName: senderName,
                brevoContactId: null,
                ct: ct);
        }

        // NEW overload that stamps BrevoContactID
        public async Task<long> EnqueueWhatsAppTemplateAsync(
            string toNumber,
            int templateId,
            Dictionary<string, object> parameters,
            string? senderName,
            int? brevoContactId,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "OUTBOX WHATSAPP ENQUEUE: To={To}, TemplateId={TemplateId}, Sender={Sender}, BrevoContactID={BrevoContactID}, Params={Params}",
                toNumber,
                templateId,
                senderName ?? "(none)",
                brevoContactId,
                JsonSerializer.Serialize(parameters ?? new(), _jsonOpts));

            var msg = new BrevoOutboxMessage
            {
                Type = BrevoOutboxType.WhatsApp,
                To = toNumber,
                TemplateId = templateId,
                ParamsJson = JsonSerializer.Serialize(parameters ?? new(), _jsonOpts),
                SenderName = senderName,
                Status = BrevoOutboxStatus.Pending,
                NextAttemptUtc = DateTimeOffset.UtcNow,

                // ✅ HERE is the required change:
                BrevoContactID = brevoContactId
            };

            _db.BrevoOutbox.Add(msg);
            await _db.SaveChangesAsync(ct);

            return msg.Id;
        }
    }
}
