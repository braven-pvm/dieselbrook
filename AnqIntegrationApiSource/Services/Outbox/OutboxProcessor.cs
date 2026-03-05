using System.Diagnostics;
using System.Text.Json;
using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Outbox;
using BrevoApiHelpers.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Services.Outbox
{
    public class OutboxProcessor : IOutboxProcessor
    {
        private readonly OutboxDbContext _db;
        private readonly IMessagingService _messaging;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(OutboxDbContext db, IMessagingService messaging, ILogger<OutboxProcessor> logger)
        {
            _db = db;
            _messaging = messaging;
            _logger = logger;
        }

        public async Task<int> ProcessBatchAsync(int maxBatchSize, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;

            var pending = await _db.BrevoOutbox
                .Where(x => x.Status == BrevoOutboxStatus.Pending && (x.NextAttemptUtc == null || x.NextAttemptUtc <= now))
                .OrderBy(x => x.NextAttemptUtc).ThenBy(x => x.Id)
                .Take(Math.Max(1, maxBatchSize))
                .ToListAsync(ct);

            if (pending.Count == 0) return 0;

            foreach (var m in pending)
            {
                ct.ThrowIfCancellationRequested();

                // mark in progress
                m.Status = BrevoOutboxStatus.InProgress;
                await _db.SaveChangesAsync(ct);

                var sw = Stopwatch.StartNew();
                var attemptNo = m.Attempts + 1;

                try
                {
                    var parameters = TryDeserialize(m.ParamsJson);
                    var toMasked = MaskPhone(m.To);

                    // Insert structured attempt record BEFORE sending
                    await InsertAttemptAsync(
                        outboxId: m.Id,
                        attemptNo: attemptNo,
                        channel: (byte)m.Type,
                        toMasked: toMasked,
                        templateId: m.TemplateId,
                        requestJson: SafeRequestJson(m.Id, m.TemplateId, toMasked, parameters.Keys),
                        ct: ct);

                    if (m.Type == BrevoOutboxType.Email && m.TemplateId is int tid)
                    {
                        var resp = await _messaging.SendTransactionalEmailAsync(m.To, tid, parameters);

                        sw.Stop();
                        await UpdateAttemptAsync(
                            outboxId: m.Id,
                            attemptNo: attemptNo,
                            responseStatus: resp.StatusCode,
                            responseBody: Trunc(resp.RawResponseBody, 4000),
                            error: Trunc(resp.Error, 512),
                            brevoMessageId: Trunc(resp.MessageId, 64),
                            durationMs: (int)sw.ElapsedMilliseconds,
                            ct: ct);

                        if (resp.Success)
                        {
                            m.Status = BrevoOutboxStatus.Sent;
                            m.SentUtc = DateTimeOffset.UtcNow;
                            m.LastError = null;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Brevo email failed: {resp.StatusCode} {resp.Error ?? resp.RawResponseBody}");
                        }
                    }
                    else if (m.Type == BrevoOutboxType.WhatsApp && m.TemplateId is int wtid)
                    {
                        var resp = await _messaging.SendWhatsappTemplateAsync(m.To, wtid, parameters);

                        sw.Stop();
                        await UpdateAttemptAsync(
                            outboxId: m.Id,
                            attemptNo: attemptNo,
                            responseStatus: resp.StatusCode,
                            responseBody: Trunc(resp.RawResponseBody, 4000),
                            error: Trunc(resp.Error, 512),
                            brevoMessageId: Trunc(resp.MessageId, 64),
                            durationMs: (int)sw.ElapsedMilliseconds,
                            ct: ct);

                        if (resp.Success)
                        {
                            m.Status = BrevoOutboxStatus.Sent;
                            m.SentUtc = DateTimeOffset.UtcNow;
                            m.LastError = null;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Brevo WhatsApp failed: {resp.StatusCode} {resp.Error ?? resp.RawResponseBody}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported outbox message.");
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();

                    // Update attempt row with error (best effort)
                    try
                    {
                        await UpdateAttemptAsync(
                            outboxId: m.Id,
                            attemptNo: attemptNo,
                            responseStatus: null,
                            responseBody: null,
                            error: Trunc(ex.Message, 512),
                            brevoMessageId: null,
                            durationMs: (int)sw.ElapsedMilliseconds,
                            ct: ct);
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogWarning(logEx, "Failed to write BrevoOutboxAttempt failure details for OutboxId={OutboxId}, AttemptNo={AttemptNo}", m.Id, attemptNo);
                    }

                    _logger.LogWarning(ex, "Outbox send failed for message {Id}", m.Id);

                    // retry/backoff
                    m.Attempts += 1;
                    m.Status = BrevoOutboxStatus.Pending;
                    m.LastError = Trunc(ex.Message, 512);

                    var delayMinutes = Math.Min(30, Math.Max(1, (int)Math.Pow(2, Math.Max(0, m.Attempts - 1))));
                    m.NextAttemptUtc = DateTimeOffset.UtcNow.AddMinutes(delayMinutes);
                }

                await _db.SaveChangesAsync(ct);
            }

            return pending.Count;
        }

        private async Task InsertAttemptAsync(
            long outboxId,
            int attemptNo,
            byte channel,
            string? toMasked,
            int? templateId,
            string? requestJson,
            CancellationToken ct)
        {
            // Note: assumes dbo.BrevoOutboxAttempt exists with these columns.
            const string sql = @"
INSERT INTO dbo.BrevoOutboxAttempt
(
    OutboxId, AttemptNo, AttemptUtc,
    Channel, ToMasked, TemplateId,
    RequestJson
)
VALUES
(
    @OutboxId, @AttemptNo, SYSUTCDATETIME(),
    @Channel, @ToMasked, @TemplateId,
    @RequestJson
);";

            var p = new[]
            {
                new SqlParameter("@OutboxId", outboxId),
                new SqlParameter("@AttemptNo", attemptNo),
                new SqlParameter("@Channel", channel),
                new SqlParameter("@ToMasked", (object?)toMasked ?? DBNull.Value),
                new SqlParameter("@TemplateId", (object?)templateId ?? DBNull.Value),
                new SqlParameter("@RequestJson", (object?)requestJson ?? DBNull.Value),
            };

            await _db.Database.ExecuteSqlRawAsync(sql, p, ct);
        }

        private async Task UpdateAttemptAsync(
            long outboxId,
            int attemptNo,
            int? responseStatus,
            string? responseBody,
            string? error,
            string? brevoMessageId,
            int? durationMs,
            CancellationToken ct)
        {
            // Update by (OutboxId, AttemptNo). If you want to enforce uniqueness,
            // add a UNIQUE index on (OutboxId, AttemptNo) in SQL.
            const string sql = @"
UPDATE dbo.BrevoOutboxAttempt
SET
    ResponseStatus = COALESCE(@ResponseStatus, ResponseStatus),
    ResponseBody   = COALESCE(@ResponseBody,   ResponseBody),
    Error          = COALESCE(@Error,          Error),
    BrevoMessageId = COALESCE(@BrevoMessageId, BrevoMessageId),
    DurationMs     = COALESCE(@DurationMs,     DurationMs)
WHERE OutboxId = @OutboxId
  AND AttemptNo = @AttemptNo;
";

            var p = new[]
            {
                new SqlParameter("@OutboxId", outboxId),
                new SqlParameter("@AttemptNo", attemptNo),
                new SqlParameter("@ResponseStatus", (object?)responseStatus ?? DBNull.Value),
                new SqlParameter("@ResponseBody", (object?)responseBody ?? DBNull.Value),
                new SqlParameter("@Error", (object?)error ?? DBNull.Value),
                new SqlParameter("@BrevoMessageId", (object?)brevoMessageId ?? DBNull.Value),
                new SqlParameter("@DurationMs", (object?)durationMs ?? DBNull.Value),
            };

            await _db.Database.ExecuteSqlRawAsync(sql, p, ct);
        }

        private static Dictionary<string, object> TryDeserialize(string json)
        {
            try { return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new(); }
            catch { return new(); }
        }

        private static string MaskPhone(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (digits.Length <= 4) return "****";
            return "****" + digits[^4..];
        }

        private static string SafeRequestJson(long outboxId, int? templateId, string toMasked, IEnumerable<string> paramKeys)
        {
            return JsonSerializer.Serialize(new
            {
                outboxId,
                templateId,
                to = toMasked,
                paramKeys = paramKeys?.ToArray() ?? Array.Empty<string>()
            });
        }

        private static string? Trunc(string? s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
