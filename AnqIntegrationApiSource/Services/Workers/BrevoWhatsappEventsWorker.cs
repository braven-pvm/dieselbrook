using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Outbox;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Net.Http.Json;

namespace AnqIntegrationApi.Services.Workers
{
    /// <summary>
    /// Polls Brevo WhatsApp events endpoint and stores ONLY events that:
    /// - Have a non-empty messageId (required for joining back to outbox)
    /// - Belong to messages we sent (BrevoOutboxAttempt.Channel=2 and BrevoMessageId=messageId exists)
    /// Uses idempotent INSERT-if-not-exists logic and ignores duplicate-key races safely.
    /// </summary>
    public class BrevoWhatsappEventsWorker : BackgroundService
    {
        private const string SyncStateName = "whatsapp_events";
        private const byte WhatsappChannel = 2;

        private readonly ILogger<BrevoWhatsappEventsWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _cfg;

        public BrevoWhatsappEventsWorker(
            ILogger<BrevoWhatsappEventsWorker> logger,
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            IConfiguration cfg)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _cfg = cfg;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _cfg.GetValue<int?>("BrevoEventsSync:IntervalMinutes") ?? 10;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncOnce(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // normal shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Brevo WhatsApp events sync failed");
                }

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }

        private async Task SyncOnce(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var nopDb = scope.ServiceProvider.GetRequiredService<NopDbContext>(); // <-- your Nop DB context
            var http = _httpClientFactory.CreateClient("Brevo");

            var safetyOverlapMin = _cfg.GetValue<int?>("BrevoEventsSync:SafetyOverlapMinutes") ?? 10;

            // Ensure sync state exists
            var state = await outboxDb.BrevoSyncStates.SingleOrDefaultAsync(x => x.Name == SyncStateName, ct);
            if (state == null)
            {
                state = new BrevoSyncState
                {
                    Name = SyncStateName,
                    LastEventUtc = DateTime.UtcNow.AddDays(-7),
                    LastStatus = "seeded"
                };
                outboxDb.BrevoSyncStates.Add(state);
                await outboxDb.SaveChangesAsync(ct);
            }

            // Apply overlap to avoid missing late events
            var effectiveFromUtc = state.LastEventUtc.AddMinutes(-safetyOverlapMin);
            if (effectiveFromUtc < DateTime.UtcNow.AddDays(-90))
                effectiveFromUtc = DateTime.UtcNow.AddDays(-90);

            // Brevo endpoint takes date-only params
            var startDate = effectiveFromUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            long offset = 0;
            const long limit = 2500;

            DateTime? maxEventUtcAny = null;   // advances watermark even if messageId missing
            DateTime? maxEventUtcInserted = null; // informative only
            int inserted = 0;
            int seen = 0;
            int skippedNoMessageId = 0;
            int skippedNotOurs = 0;

            while (!ct.IsCancellationRequested)
            {
                var uri = $"whatsapp/statistics/events?startDate={startDate}&endDate={endDate}&limit={limit}&offset={offset}&sort=asc";
                using var resp = await http.GetAsync(uri, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"Brevo events API failed: {(int)resp.StatusCode} {resp.ReasonPhrase} {body}");

                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("events", out var eventsEl) || eventsEl.ValueKind != JsonValueKind.Array)
                    break;

                var batchCount = eventsEl.GetArrayLength();
                if (batchCount == 0) break;

                foreach (var e in eventsEl.EnumerateArray())
                {
                    ct.ThrowIfCancellationRequested();
                    seen++;

                    var dateStr = e.TryGetProperty("date", out var d) ? d.GetString() : null;
                    var evType = e.TryGetProperty("event", out var ev) ? ev.GetString() : null;

                    if (string.IsNullOrWhiteSpace(dateStr) || string.IsNullOrWhiteSpace(evType))
                        continue;

                    if (!DateTime.TryParse(dateStr, null, DateTimeStyles.AdjustToUniversal, out var eventUtc))
                        continue;

                    // track max event time even if we skip/ignore (prevents stalling forever)
                    if (!maxEventUtcAny.HasValue || eventUtc > maxEventUtcAny.Value)
                        maxEventUtcAny = eventUtc;

                    var msgId = e.TryGetProperty("messageId", out var mid) ? mid.GetString() : null;
                    if (string.IsNullOrWhiteSpace(msgId))
                    {
                        skippedNoMessageId++;
                        continue; // must have messageId for joining
                    }

                    var sender = e.TryGetProperty("senderNumber", out var sn) ? sn.GetString() : null;
                    var contact = e.TryGetProperty("contactNumber", out var cn) ? cn.GetString() : null;
                    var replyBody = e.TryGetProperty("body", out var b) ? b.GetString() : null;
                    var mediaUrl = e.TryGetProperty("mediaUrl", out var mu) ? mu.GetString() : null;
                    var raw = e.GetRawText();

                    var rows = await InsertEventIfNotExistsForOurOutboxAsync(
                        outboxDb,
                        eventUtc,
                        evType!,
                        msgId!,
                        sender,
                        contact,
                        replyBody,
                        mediaUrl,
                        raw,
                        ct);




                    if (rows == -1)
                    {
                        // not ours (no matching outbox attempt)
                        skippedNotOurs++;
                        continue;
                    }


                    if (rows != -1 && evType!.Equals("unsubscribe", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessUnsubscribeAsync(outboxDb,nopDb, http, msgId!, ct);
                    }

                    inserted += rows;

                    if (rows > 0 && (!maxEventUtcInserted.HasValue || eventUtc > maxEventUtcInserted.Value))
                        maxEventUtcInserted = eventUtc;
                }

                offset += limit;
                if (batchCount < limit) break;
            }

            // Update watermark:
            // Use maxEventUtcAny so we don't refetch the same "no messageId" events forever.
            state.LastRunUtc = DateTime.UtcNow;
            state.LastError = null;
            state.LastStatus = $"ok (seen {seen}, inserted {inserted}, skippedNoMessageId {skippedNoMessageId}, skippedNotOurs {skippedNotOurs})";

            if (maxEventUtcAny.HasValue && maxEventUtcAny.Value > state.LastEventUtc)
                state.LastEventUtc = maxEventUtcAny.Value;

            await outboxDb.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Brevo WhatsApp events sync OK. start={Start} end={End} seen={Seen} inserted={Inserted} skippedNoMessageId={SkipNoId} skippedNotOurs={SkipNotOurs} watermark={Watermark:o}",
                startDate, endDate, seen, inserted, skippedNoMessageId, skippedNotOurs, state.LastEventUtc);
        }

        public static async Task<UnsubscribeProcessResult> ProcessUnsubscribeByEventIdAsync(
    OutboxDbContext outboxDb,
    NopDbContext nopDb,
    HttpClient brevoHttp,
    long brevoWhatsappEventId,
    CancellationToken ct)
        {
            // Load event
            const string getSql = @"
SELECT TOP (1)
    Id,
    EventType,
    MessageId
FROM dbo.BrevoWhatsappEvent
WHERE Id = @Id;";

            await using var conn = outboxDb.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = getSql;

            var pId = cmd.CreateParameter();
            pId.ParameterName = "@Id";
            pId.Value = brevoWhatsappEventId;
            cmd.Parameters.Add(pId);

            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            if (!await rdr.ReadAsync(ct))
            {
                return new UnsubscribeProcessResult(
                    EventId: brevoWhatsappEventId,
                    MessageId: null,
                    Status: "NotFound",
                    BrevoContactId: null,
                    Error: "BrevoWhatsappEvent not found.");
            }

            var eventType = rdr["EventType"]?.ToString();
            var messageId = rdr["MessageId"]?.ToString();

            if (!string.Equals(eventType, "unsubscribe", StringComparison.OrdinalIgnoreCase))
            {
                return new UnsubscribeProcessResult(
                    EventId: brevoWhatsappEventId,
                    MessageId: messageId,
                    Status: "NotUnsubscribe",
                    BrevoContactId: null,
                    Error: $"EventType is '{eventType}', expected 'unsubscribe'.");
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                return new UnsubscribeProcessResult(
                    EventId: brevoWhatsappEventId,
                    MessageId: null,
                    Status: "NoMessageId",
                    BrevoContactId: null,
                    Error: "Event has no MessageId; cannot join back to outbox.");
            }

            // Pre-resolve BrevoContactId for reporting (optional)
            int? brevoContactId = null;
            try
            {
                brevoContactId = await LookupBrevoContactIdAsync(outboxDb, messageId, ct);
            }
            catch { /* ignore; ProcessUnsubscribeAsync will handle */ }

            try
            {
                // Reuse the same logic the worker uses.
                // This will only do work if there is an unprocessed unsubscribe event for this messageId.
                await ProcessUnsubscribeAsync(outboxDb, nopDb, brevoHttp, messageId, ct);

                // If it was already processed, ProcessUnsubscribeAsync returns without changes.
                // We can check ProcessedUtc to differentiate if you want, but "Ok" is usually enough for testing.
                return new UnsubscribeProcessResult(
                    EventId: brevoWhatsappEventId,
                    MessageId: messageId,
                    Status: "Ok",
                    BrevoContactId: brevoContactId,
                    Error: null);
            }
            catch (Exception ex)
            {
                return new UnsubscribeProcessResult(
                    EventId: brevoWhatsappEventId,
                    MessageId: messageId,
                    Status: "Failed",
                    BrevoContactId: brevoContactId,
                    Error: ex.Message);
            }
        }

        /// <summary>
        /// Inserts event only if:
        /// - MessageId is not empty
        /// - MessageId exists in our outbox attempt log (Channel=2)
        /// - Event is not already stored (dedupe by EventUtc+EventType+MessageId)
        ///
        /// Returns:
        ///  1 => inserted
        ///  0 => already exists / duplicate
        /// -1 => not ours (no matching outbox attempt)
        /// </summary>
        private static async Task<int> InsertEventIfNotExistsForOurOutboxAsync(
            OutboxDbContext db,
            DateTime eventUtc,
            string eventType,
            string messageId,
            string? senderNumber,
            string? contactNumber,
            string? body,
            string? mediaUrl,
            string rawJson,
            CancellationToken ct)
        {
            // First, ensure this messageId belongs to OUR outbox messages (fast guard).
            const string oursSql = @"
SELECT TOP(1) 1
FROM dbo.BrevoOutboxAttempt a
WHERE a.Channel = @Channel
  AND a.BrevoMessageId = @MessageId;";

            var oursParams = new[]
            {
                new SqlParameter("@Channel", WhatsappChannel),
                new SqlParameter("@MessageId", messageId),
            };

        
            const string insertSql = @"
INSERT INTO dbo.BrevoWhatsappEvent
    (EventUtc, EventType, MessageId, SenderNumber, ContactNumber, Body, MediaUrl, RawJson)
SELECT
    @EventUtc, @EventType, @MessageId, @SenderNumber, @ContactNumber, @Body, @MediaUrl, @RawJson
WHERE
    NULLIF(LTRIM(RTRIM(@MessageId)), '') IS NOT NULL
    AND EXISTS
    (
        SELECT 1
        FROM dbo.BrevoOutboxAttempt a
        WHERE a.Channel = @Channel
          AND a.BrevoMessageId = @MessageId
    )
    AND NOT EXISTS
    (
        SELECT 1
        FROM dbo.BrevoWhatsappEvent WITH (UPDLOCK, HOLDLOCK)
        WHERE EventUtc = @EventUtc
          AND EventType = @EventType
          AND MessageId = @MessageId
    );";

            var p = new[]
            {
                new SqlParameter("@EventUtc", eventUtc),
                new SqlParameter("@EventType", eventType),
                new SqlParameter("@MessageId", messageId),
                new SqlParameter("@SenderNumber", (object?)senderNumber ?? DBNull.Value),
                new SqlParameter("@ContactNumber", (object?)contactNumber ?? DBNull.Value),
                new SqlParameter("@Body", (object?)body ?? DBNull.Value),
                new SqlParameter("@MediaUrl", (object?)mediaUrl ?? DBNull.Value),
                new SqlParameter("@RawJson", rawJson),
                new SqlParameter("@Channel", WhatsappChannel),
            };

            try
            {
                var rows = await db.Database.ExecuteSqlRawAsync(insertSql, p, ct);
                if (rows > 0) return rows;

                // rows==0 could be: duplicate already stored OR not ours.
                // Determine "not ours" so caller can count it.
                // (This avoids scanning BrevoWhatsappEvent when it was simply not ours.)
                var ours = await ExistsAsync(db, oursSql, oursParams, ct);
                return ours ? 0 : -1;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // Unique key race - ignore
                return 0;
            }
        }

        /// <summary>
        /// Executes a simple EXISTS query and returns true/false using ADO via DbConnection.
        /// Keeps everything in this file; avoids keyless EF entities.
        /// </summary>
        private static async Task<bool> ExistsAsync(
    OutboxDbContext db,
    string sql,
    SqlParameter[] parameters,
    CancellationToken ct)
        {
            var conn = db.Database.GetDbConnection();

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;

            foreach (var p in parameters)
                cmd.Parameters.Add(p);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null && result != DBNull.Value;
        }

        private static async Task ProcessUnsubscribeAsync(
            OutboxDbContext outboxDb,
            NopDbContext nopDb,
            HttpClient brevoHttp,
            string messageId,
            CancellationToken ct)
        {
            // Claim one unprocessed unsubscribe event row for this messageId.
            // This makes it idempotent and safe even if the worker restarts or re-reads events.
            var eventId = await TryClaimUnprocessedUnsubscribeEventAsync(outboxDb, messageId, ct);
            if (eventId == null) return; // nothing to do

            try
            {
                // 1) Lookup BrevoContactID via OutboxAttempt -> Outbox
                var brevoContactId = await LookupBrevoContactIdAsync(outboxDb, messageId, ct);
                if (brevoContactId == null)
                {
                    // Not expected since it's "ours", but handle safely.
                    await MarkEventFailedAsync(outboxDb, eventId.Value, "Could not resolve BrevoContactID for messageId.", ct, clearProcessedUtc: true);
                    return;
                }

                // 2) Update Brevo: whatsappBlacklisted + WHATSAPP_CONSENT = No
                await UpdateBrevoContactOptOutAsync(brevoHttp, brevoContactId.Value, ct);

                // 3) Update Nop additional info (join on BrevoID)
                await UpdateNopWhatsappOptOutAsync(nopDb, brevoContactId.Value, ct);

                // Mark processed OK (leave ProcessedUtc as set by claim, clear error)
                await MarkEventSucceededAsync(outboxDb, eventId.Value, ct);
            }
            catch (Exception ex)
            {
                // Store error and allow retry next run by clearing ProcessedUtc
                await MarkEventFailedAsync(outboxDb, eventId.Value, ex.ToString(), ct, clearProcessedUtc: true);
                throw;
            }
        }

        /// <summary>
        /// Claims an unprocessed unsubscribe event row for this messageId by setting ProcessedUtc.
        /// Returns the claimed event row Id, or null if none to claim.
        /// </summary>
        private static async Task<long?> TryClaimUnprocessedUnsubscribeEventAsync(
            OutboxDbContext db,
            string messageId,
            CancellationToken ct)
        {
            // NOTE:
            // - READPAST skips rows locked by another transaction
            // - UPDLOCK + ROWLOCK helps prevent race conditions
            // - We "claim" by setting ProcessedUtc now; on failure we clear it so it retries.
            const string sql = @"
DECLARE @Claimed TABLE (Id BIGINT);

UPDATE TOP (1) e WITH (ROWLOCK, UPDLOCK, READPAST)
SET
    e.ProcessedUtc = SYSUTCDATETIME(),
    e.ProcessedError = NULL
OUTPUT inserted.Id INTO @Claimed(Id)
FROM dbo.BrevoWhatsappEvent e
WHERE e.MessageId = @MessageId
  AND e.EventType = N'unsubscribe'
  AND e.ProcessedUtc IS NULL
ORDER BY e.EventUtc ASC;

SELECT TOP (1) Id FROM @Claimed;";

            var idObj = await ScalarAsync(
                db,
                sql,
                new[]
                {
            new SqlParameter("@MessageId", messageId),
                },
                ct);

            return idObj == null ? null : Convert.ToInt64(idObj);
        }

        /// <summary>
        /// Gets BrevoContactID by joining BrevoOutboxAttempt -> BrevoOutbox.
        /// </summary>
        private static async Task<int?> LookupBrevoContactIdAsync(
            OutboxDbContext db,
            string messageId,
            CancellationToken ct)
        {
            const string sql = @"
SELECT TOP (1)
    ob.BrevoContactID
FROM dbo.BrevoOutboxAttempt a
JOIN dbo.BrevoOutbox ob ON ob.Id = a.OutboxId
WHERE a.Channel = @Channel
  AND a.BrevoMessageId = @MessageId
  AND ob.BrevoContactID IS NOT NULL
ORDER BY a.Id DESC;";

            var obj = await ScalarAsync(
                db,
                sql,
                new[]
                {
            new SqlParameter("@Channel", WhatsappChannel),
            new SqlParameter("@MessageId", messageId),
                },
                ct);

            return obj == null ? null : Convert.ToInt32(obj);
        }

        private static async Task UpdateBrevoContactOptOutAsync(
            HttpClient brevoHttp,
            int brevoContactId,
            CancellationToken ct)
        {
            // Single call does both:
            // 1) whatsappBlacklisted = true
            // 2) WHATSAPP_CONSENT = "No"
            var payload = new
            {
                whatsappBlacklisted = true,
                attributes = new Dictionary<string, object>
                {
                    ["WHATSAPP_CONSENT"] = "No"
                }
            };

            using var resp = await brevoHttp.PutAsJsonAsync($"contacts/{brevoContactId}", payload, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Brevo contact update failed for {brevoContactId}: {(int)resp.StatusCode} {resp.ReasonPhrase} {body}");
        }

        private static async Task UpdateNopWhatsappOptOutAsync(
           NopDbContext nopDb,
           int brevoContactId,
           CancellationToken ct)
        {
            const string sql = @"
UPDATE dbo.ANQ_UserProfileAdditionalInfo
SET
    whatsappoptin = 0,
    whatsappoptinsource = 'Unsubscribed',
    whatsappoptindate = SYSUTCDATETIME()
WHERE BrevoID = @BrevoID;";

            await nopDb.Database.ExecuteSqlRawAsync(
                sql,
                new SqlParameter("@BrevoID", brevoContactId),
                ct);
        }

        private static async Task MarkEventSucceededAsync(
            OutboxDbContext db,
            long eventId,
            CancellationToken ct)
        {
            const string sql = @"
UPDATE dbo.BrevoWhatsappEvent
SET ProcessedError = NULL
WHERE Id = @Id;";

            await db.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@Id", eventId), ct);
        }

        private static async Task MarkEventFailedAsync(
            OutboxDbContext db,
            long eventId,
            string error,
            CancellationToken ct,
            bool clearProcessedUtc)
        {
            const string sql = @"
UPDATE dbo.BrevoWhatsappEvent
SET
    ProcessedError = @Err,
    ProcessedUtc = CASE WHEN @Clear = 1 THEN NULL ELSE ProcessedUtc END
WHERE Id = @Id;";

            await db.Database.ExecuteSqlRawAsync(
                sql,
                new SqlParameter("@Id", eventId),
                new SqlParameter("@Err", error),
                new SqlParameter("@Clear", clearProcessedUtc ? 1 : 0),
                ct);
        }

        private static async Task<object?> ScalarAsync(
            OutboxDbContext db,
            string sql,
            SqlParameter[] parameters,
            CancellationToken ct)
        {
            var conn = db.Database.GetDbConnection();

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;

            foreach (var p in parameters)
                cmd.Parameters.Add(p);

            return await cmd.ExecuteScalarAsync(ct);
        }

    }



    public sealed record UnsubscribeProcessResult(
    long EventId,
    string? MessageId,
    string Status,
    int? BrevoContactId,
    string? Error);
}
