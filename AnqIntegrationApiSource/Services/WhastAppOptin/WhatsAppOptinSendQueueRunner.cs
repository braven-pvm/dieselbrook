using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Outbox;
using AnqIntegrationApi.Models.Settings;
using AnqIntegrationApi.Services.Messaging;
using BrevoApiHelpers.Models;
using BrevoApiHelpers.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AnqIntegrationApi.Services.WhatsAppOptin;

public interface IWhatsAppOptinSendQueueRunner
{
    Task<RunOnceResult> RunOnceAsync(int batchSize, string lockedBy, CancellationToken ct);
}

public sealed class RunOnceResult
{
    public int Claimed { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Retried { get; set; }
}

public sealed class WhatsAppOptinSendQueueRunner : IWhatsAppOptinSendQueueRunner
{
    private readonly OutboxDbContext _outboxDb;
    private readonly ApiSettingsDbContext _settingsDb;
    private readonly IContactService _contacts;
    private readonly IMessagingDispatcher _dispatcher;
    private readonly BrevoSettings _brevoSettings;
    private readonly ILogger<WhatsAppOptinSendQueueRunner> _logger;

    private readonly string _workerName;

    private const int DefaultTtlMinutes = 7200;
    private const string Purpose = "WHATSAPP_OPTIN";

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public WhatsAppOptinSendQueueRunner(
        OutboxDbContext outboxDb,
        ApiSettingsDbContext settingsDb,
        IContactService contacts,
        IMessagingDispatcher dispatcher,
        IOptions<BrevoSettings> brevoOptions,
        ILogger<WhatsAppOptinSendQueueRunner> logger)
    {
        _outboxDb = outboxDb;
        _settingsDb = settingsDb;
        _contacts = contacts;
        _dispatcher = dispatcher;
        _brevoSettings = brevoOptions.Value;
        _logger = logger;

        _workerName = $"{Environment.MachineName}:{Guid.NewGuid():N}".Substring(0, 32);
    }

    public async Task<RunOnceResult> RunOnceAsync(int batchSize, string lockedBy, CancellationToken ct)
    {
        if (batchSize < 1) batchSize = 1;
        if (batchSize > 200) batchSize = 200;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Runner"] = nameof(WhatsAppOptinSendQueueRunner),
            ["Worker"] = _workerName,
            ["BatchSize"] = batchSize,
            ["LockedBy"] = lockedBy
        }))
        {
            // Fail fast: if DI gave us an empty connection string, stop immediately.
            LogDbConnection(_logger, _outboxDb, "RunnerStart");

            var c = _outboxDb.Database.GetDbConnection();
            if (string.IsNullOrWhiteSpace(c.ConnectionString))
            {
                _logger.LogCritical("OUTBOX DB CONNECTION STRING IS EMPTY AT RUN START. Aborting RunOnce.");
                throw new InvalidOperationException("OutboxDbContext resolved with empty connection string.");
            }

            var result = new RunOnceResult();

            _logger.LogInformation("RunOnce starting.");

            List<WhatsAppOptinSendQueueItem> items;
            try
            {
                items = await ClaimBatchAsync(batchSize, lockedBy, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClaimBatch failed.");
                throw;
            }

            result.Claimed = items.Count;

            if (items.Count == 0)
            {
                _logger.LogInformation("RunOnce finished. Claimed=0");
                return result;
            }

            _logger.LogInformation("RunOnce claimed {Count} item(s).", items.Count);

            foreach (var item in items)
            {
                if (ct.IsCancellationRequested)
                    break;

                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["QueueId"] = item.Id,
                    ["ApiClientId"] = item.ApiClientId,
                    ["Attempt"] = item.AttemptCount,
                    ["Status"] = item.Status
                }))
                {
                    var sw = Stopwatch.StartNew();
                    var beforeAttempt = item.AttemptCount;

                    _logger.LogInformation(
                        "Processing queue item. Status={Status} AttemptCount={AttemptCount} Priority={Priority} CreatedUtc={CreatedUtc}",
                        item.Status, item.AttemptCount, item.Priority, item.CreatedUtc);

                    try
                    {
                        // Connection snapshot before processing
                        LogDbConnection(_logger, _outboxDb, "BeforeProcessOne");

                        await ProcessOneAsync(
                            _outboxDb,
                            _settingsDb,
                            _contacts,
                            _dispatcher,
                            _brevoSettings,
                            item,
                            ct);

                        sw.Stop();
                        _logger.LogInformation("ProcessOne completed in {ElapsedMs}ms.", sw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        _logger.LogError(ex, "FAILED processing queue item after {ElapsedMs}ms.", sw.ElapsedMilliseconds);

                        // Try to mark retry/fail; if THIS fails, log loudly because item can remain 'Sending'.
                        try
                        {
                            LogDbConnection(_logger, _outboxDb, "RetryAfterFailure");
                            await MarkRetryOrFailAsync(_outboxDb, item.Id, ex.Message, ct);
                        }
                        catch (Exception retryEx)
                        {
                            _logger.LogCritical(
                                retryEx,
                                "FAILED to update queue status after processing error. ITEM MAY REMAIN 'Sending'.");
                        }
                    }

                    // reload to infer outcome (lightweight)
                    try
                    {
                        var latest = await _outboxDb.WhatsAppOptinSendQueue
                            .AsNoTracking()
                            .Where(x => x.Id == item.Id)
                            .Select(x => new { x.Status, x.AttemptCount })
                            .FirstOrDefaultAsync(ct);

                        if (latest != null)
                        {
                            if (latest.Status == "Sent") result.Sent++;
                            else if (latest.Status == "Failed") result.Failed++;
                            else if (latest.Status == "Send" && latest.AttemptCount > beforeAttempt) result.Retried++;

                            _logger.LogInformation(
                                "Post-process state: Status={Status} AttemptCount={AttemptCount}",
                                latest.Status, latest.AttemptCount);
                        }
                        else
                        {
                            _logger.LogWarning("Post-process reload returned null (queue row not found).");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to reload queue row for outcome inference.");
                    }
                }
            }

            _logger.LogInformation(
                "RunOnce finished. Claimed={Claimed} Sent={Sent} Failed={Failed} Retried={Retried}",
                result.Claimed, result.Sent, result.Failed, result.Retried);

            return result;
        }
    }

    // ---------------------------
    // CLAIM (raw sql) - includes reclaim stuck "Sending"
    // ---------------------------
    private async Task<List<WhatsAppOptinSendQueueItem>> ClaimBatchAsync(int batchSize, string lockedBy, CancellationToken ct)
    {
        _logger.LogInformation("Claiming WhatsAppOptinSendQueue batch. BatchSize={BatchSize} LockedBy={LockedBy}", batchSize, lockedBy);

        LogDbConnection(_logger, _outboxDb, "BeforeClaimBatch");

        await using var tx = await _outboxDb.Database.BeginTransactionAsync(ct);

        var ids = await _outboxDb.Database.SqlQueryRaw<Guid>($@"
SELECT TOP ({batchSize}) q.Id
FROM dbo.WhatsAppOptinSendQueue q WITH (READPAST, UPDLOCK, ROWLOCK)
WHERE (
        q.Status = 'Send'
     OR (q.Status = 'Sending' AND q.LockedUtc < DATEADD(MINUTE, -10, SYSUTCDATETIME()))
      )
  AND (q.CustomerId IS NOT NULL OR NULLIF(LTRIM(RTRIM(q.Username)), '') IS NOT NULL)
ORDER BY q.Priority ASC, q.CreatedUtc ASC
").ToListAsync(ct);

        _logger.LogInformation("ClaimBatch found {Count} item id(s).", ids.Count);

        if (ids.Count == 0)
        {
            await tx.CommitAsync(ct);
            return new List<WhatsAppOptinSendQueueItem>();
        }

        var idList = string.Join(",", ids.Select(x => $"'{x:D}'"));

        await _outboxDb.Database.ExecuteSqlRawAsync($@"
UPDATE dbo.WhatsAppOptinSendQueue
SET Status = 'Sending',
    LockedUtc = SYSUTCDATETIME(),
    LockedBy = {{0}},
    UpdatedUtc = SYSUTCDATETIME()
WHERE Id IN ({idList});
", new object[] { lockedBy }, ct);

        _logger.LogInformation("ClaimBatch marked {Count} item(s) as Sending.", ids.Count);

        await tx.CommitAsync(ct);

        var claimed = await _outboxDb.WhatsAppOptinSendQueue
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedUtc)
            .ToListAsync(ct);

        _logger.LogInformation("ClaimBatch loaded {Count} item(s) for processing.", claimed.Count);

        return claimed;
    }

    // ---------------------------
    // PROCESS ONE
    // ---------------------------
    private async Task ProcessOneAsync(
        OutboxDbContext outboxDb,
        ApiSettingsDbContext settingsDb,
        IContactService contacts,
        IMessagingDispatcher dispatcher,
        BrevoSettings brevoSettings,
        WhatsAppOptinSendQueueItem item,
        CancellationToken ct)
    {
        try
        {
            LogDbConnection(_logger, outboxDb, "ProcessOne:Start");

            // Skip bad identifiers
            if ((!item.CustomerId.HasValue || item.CustomerId.Value <= 0) && string.IsNullOrWhiteSpace(item.Username))
            {
                _logger.LogWarning("Invalid identifiers (CustomerId/Username missing).");
                await MarkFailedAsync(outboxDb, item.Id, "CustomerId and Username are both missing.", ct);
                return;
            }

            // ApiClient config
            _logger.LogInformation("Loading ApiClient config (ApiClientId={ApiClientId})", item.ApiClientId);

            var client = await settingsDb.ApiClients
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == item.ApiClientId, ct);

            if (client == null)
            {
                await MarkFailedAsync(outboxDb, item.Id, $"ApiClient not found (ApiClientId={item.ApiClientId}).", ct);
                return;
            }

            if (string.IsNullOrWhiteSpace(client.NopDbConnection))
            {
                await MarkFailedAsync(outboxDb, item.Id, $"ApiClient has no NopDbConnection (ApiClientId={item.ApiClientId}).", ct);
                return;
            }

            var listId = item.BrevoListId ?? brevoSettings.WhatsAppOptinListId;
            if (listId <= 0)
            {
                await MarkFailedAsync(outboxDb, item.Id, "Brevo WhatsAppOptinListId not configured (and no override on queue row).", ct);
                return;
            }

            if (string.IsNullOrWhiteSpace(item.BaseUrl))
            {
                await MarkFailedAsync(outboxDb, item.Id, "BaseUrl is missing.", ct);
                return;
            }

            if (item.EmailTemplateId <= 0)
            {
                await MarkFailedAsync(outboxDb, item.Id, "EmailTemplateId is missing/invalid.", ct);
                return;
            }

            _logger.LogInformation("Building NopDbContext (CustomerId={CustomerId}, Username={Username})", item.CustomerId, item.Username);

            // Build Nop context
            var nopOptions = new DbContextOptionsBuilder<NopDbContext>()
                .UseSqlServer(client.NopDbConnection)
                .Options;

            await using var nopDb = new NopDbContext(nopOptions);

            // Resolve CustomerId
            var resolvedCustomerId = await ResolveCustomerIdAsync(item, nopDb, ct);
            if (!resolvedCustomerId.HasValue || resolvedCustomerId.Value <= 0)
            {
                await MarkFailedAsync(outboxDb, item.Id, "Unable to resolve CustomerId from CustomerId/Username.", ct);
                return;
            }

            _logger.LogInformation("Resolved CustomerId={CustomerId}", resolvedCustomerId.Value);

            // Load customer details from Nop.Customer
            var cust = await nopDb.Customers
                .AsNoTracking()
                .Where(c => c.Id == resolvedCustomerId.Value)
                .Select(c => new
                {
                    c.Email,
                    c.FirstName,
                    c.LastName,
                    c.Phone
                })
                .FirstOrDefaultAsync(ct);

            if (cust == null)
            {
                await MarkFailedAsync(outboxDb, item.Id, $"Customer not found in Nop (CustomerId={resolvedCustomerId.Value}).", ct);
                return;
            }

            var email = (item.Email ?? cust.Email)?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                await MarkFailedAsync(outboxDb, item.Id, $"Customer email missing (CustomerId={resolvedCustomerId.Value}).", ct);
                return;
            }

            // Compose contact fields
            var firstName = (item.FirstName ?? cust.FirstName ?? "").Trim();
            var lastName = (item.LastName ?? cust.LastName ?? "").Trim();
            var phone = (item.Phone ?? cust.Phone ?? "").Trim();

            if (string.IsNullOrWhiteSpace(firstName)) firstName = "-";
            if (string.IsNullOrWhiteSpace(lastName)) lastName = "-";
            phone ??= "";
            if (phone == "-") phone = "";

            _logger.LogInformation(
                "Customer details: Email={Email} FirstName={FirstName} LastName={LastName} PhoneLen={PhoneLen}",
                email, firstName, lastName, phone?.Length ?? 0);

            // Create token
            var tokenId = Guid.NewGuid();
            var token = GenerateToken();
            var tokenHash = Sha256Base64(token);
            var expiresUtc = DateTime.UtcNow.AddMinutes(DefaultTtlMinutes);

            outboxDb.WhatsappOptInTokens.Add(new WhatsappOptInToken
            {
                Id = tokenId,
                ApiClientId = item.ApiClientId,
                CustomerId = resolvedCustomerId.Value,
                Email = email,
                TokenHash = tokenHash,
                Purpose = Purpose,
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = expiresUtc,
                UsedUtc = null
            });

            LogDbConnection(_logger, outboxDb, "BeforeSave:TokenInsert");
            await outboxDb.SaveChangesAsync(ct);

            // Link
            var baseUrl = item.BaseUrl.Trim();
            var link = $"{baseUrl}?id={tokenId:D}&t={Uri.EscapeDataString(token)}";

            // Tracking row
            var requestNumber = await GetNextOptinRequestNumberAsync(outboxDb, ct);

            outboxDb.WhatsAppOptinRequests.Add(new WhatsAppOptinRequest
            {
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = null,

                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                WhatsApp = null,

                BrevoListId = listId,
                EmailTemplateId = item.EmailTemplateId,

                Token = token,
                TokenExpiresOnUtc = expiresUtc,

                RequestNumber = requestNumber,
                Status = "Pending",

                ConsumedOnUtc = null,
                ConsumeIp = null,
                ConsumeUserAgent = null
            });

            LogDbConnection(_logger, outboxDb, "BeforeSave:RequestInsert");
            await outboxDb.SaveChangesAsync(ct);

            // Upsert Brevo contact + add to list
            _logger.LogInformation("Upserting Brevo contact + adding to list {ListId}", listId);

            var brevoUpsert = await contacts.AddContactAsync(
                new ContactModel
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    WhatsApp = "" // safe because ContactService omits empty
                },
                new List<int> { listId });

            if (!brevoUpsert.Success)
            {
                _logger.LogWarning("Brevo upsert failed. Status={Status} Error={Error}", brevoUpsert.StatusCode, brevoUpsert.Error);
                await MarkRetryOrFailAsync(outboxDb, item.Id, $"Brevo upsert failed: {brevoUpsert.Error}", ct);
                return;
            }

            // Send email
            var parameters = new Dictionary<string, object>
            {
                ["id"] = tokenId.ToString("D"),
                ["token"] = token,
                ["link"] = link,
                ["email"] = email,
                ["firstName"] = firstName,
                ["lastName"] = lastName
            };

            _logger.LogInformation(
                "Dispatching email. TemplateId={TemplateId} ParamKeys=[{Keys}]",
                item.EmailTemplateId,
                string.Join(",", parameters.Keys));

            await dispatcher.SendEmailAsync(email, item.EmailTemplateId, parameters, senderName: null, ct: ct);

            await MarkSentAsync(outboxDb, item.Id, ct);

            _logger.LogInformation(
                "Opt-in email processed OK. CustomerId={CustomerId} Email={Email} RequestNumber={RequestNumber}",
                resolvedCustomerId.Value, email, requestNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing queue item.");
            await MarkRetryOrFailAsync(outboxDb, item.Id, ex.Message, ct);
        }
    }

    private static async Task<int?> ResolveCustomerIdAsync(WhatsAppOptinSendQueueItem item, NopDbContext nopDb, CancellationToken ct)
    {
        if (item.CustomerId.HasValue && item.CustomerId.Value > 0)
            return item.CustomerId.Value;

        var username = item.Username?.Trim();
        if (!string.IsNullOrWhiteSpace(username))
        {
            return await nopDb.Customers
                .AsNoTracking()
                .Where(c => c.Username == username)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(ct);
        }

        return null;
    }

    private static async Task<int> GetNextOptinRequestNumberAsync(OutboxDbContext outboxDb, CancellationToken ct)
    {
        // NOTE: Keeping your original code. (You previously found EF sometimes wraps NEXT VALUE FOR oddly.)
        try
        {
            var next = await outboxDb.Database
                .SqlQueryRaw<int>("SELECT NEXT VALUE FOR dbo.Seq_WhatsAppOptinRequestNumber AS Value")
                .SingleAsync(ct);

            if (next > 0)
                return next;
        }
        catch
        {
            // ignore and fallback
        }

        await using var tx = await outboxDb.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

        var max = await outboxDb.WhatsAppOptinRequests.MaxAsync(x => (int?)x.RequestNumber, ct) ?? 0;
        var nextNumber = max + 1;

        await tx.CommitAsync(ct);
        return nextNumber;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string Sha256Base64(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private async Task MarkSentAsync(OutboxDbContext outboxDb, Guid id, CancellationToken ct)
    {
        var row = await outboxDb.WhatsAppOptinSendQueue.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null) return;

        row.Status = "Sent";
        row.SentUtc = DateTime.UtcNow;
        row.UpdatedUtc = DateTime.UtcNow;
        row.LastError = null;
        row.LockedUtc = null;
        row.LockedBy = null;

        LogDbConnection(_logger, outboxDb, "BeforeSave:MarkSent");
        await outboxDb.SaveChangesAsync(ct);
    }

    private async Task MarkFailedAsync(OutboxDbContext outboxDb, Guid id, string error, CancellationToken ct)
    {
        var row = await outboxDb.WhatsAppOptinSendQueue.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null) return;

        row.Status = "Failed";
        row.AttemptCount += 1;
        row.LastAttemptUtc = DateTime.UtcNow;
        row.LastError = Trunc(error, 1000);
        row.UpdatedUtc = DateTime.UtcNow;
        row.LockedUtc = null;
        row.LockedBy = null;

        LogDbConnection(_logger, outboxDb, "BeforeSave:MarkFailed");
        await outboxDb.SaveChangesAsync(ct);
    }

    private async Task MarkRetryOrFailAsync(OutboxDbContext outboxDb, Guid id, string error, CancellationToken ct)
    {
        const int maxAttempts = 3;

        var row = await outboxDb.WhatsAppOptinSendQueue.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null) return;

        row.AttemptCount += 1;
        row.LastAttemptUtc = DateTime.UtcNow;
        row.LastError = Trunc(error, 1000);
        row.UpdatedUtc = DateTime.UtcNow;

        row.Status = (row.AttemptCount >= maxAttempts) ? "Failed" : "Send";
        row.LockedUtc = null;
        row.LockedBy = null;

        LogDbConnection(_logger, outboxDb, "BeforeSave:MarkRetryOrFail");
        await outboxDb.SaveChangesAsync(ct);
    }

    private static string Trunc(string s, int max) =>
        string.IsNullOrWhiteSpace(s) ? "" : (s.Length <= max ? s : s[..max]);

    private static void LogDbConnection(ILogger logger, OutboxDbContext db, string stage)
    {
        try
        {
            var c = db.Database.GetDbConnection();
            logger.LogInformation(
                "OUTBOX DB [{Stage}] | csLen={Len} DS='{DS}' DB='{DB}'",
                stage,
                c.ConnectionString?.Length ?? 0,
                c.DataSource,
                c.Database
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read OUTBOX DB connection info at stage {Stage}", stage);
        }
    }
}
