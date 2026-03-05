using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Outbox;
using AnqIntegrationApi.Models.Settings;
using AnqIntegrationApi.Services.Messaging;
using BrevoApiHelpers.Models;
using BrevoApiHelpers.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.Common;

namespace AnqIntegrationApi.Workers;

public sealed class WhatsAppOptinEmailWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WhatsAppOptinEmailWorker> _logger;

    private readonly string _workerName;

    private const int DefaultTtlMinutes = 7200;
    private const string Purpose = "WHATSAPP_OPTIN";

    public WhatsAppOptinEmailWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<WhatsAppOptinEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _workerName = $"{Environment.MachineName}:{Guid.NewGuid():N}".Substring(0, 32);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WhatsAppOptinEmailWorker started. Worker={Worker}", _workerName);
  

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await RunOnceAsync(_scopeFactory, _logger, _workerName, batchSize: 25, stoppingToken);

                if (processed == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker loop error.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("WhatsAppOptinEmailWorker stopped. Worker={Worker}", _workerName);
    }

    /// <summary>
    /// Run one batch (claim -> process) once. Useful for controller-driven runs in DEV.
    /// Returns number of items claimed (attempted).
    /// </summary>
    public static async Task<int> RunOnceAsync(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        string workerName,
        int batchSize,
        CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        var settingsDb = scope.ServiceProvider.GetRequiredService<ApiSettingsDbContext>();

        var brevoSettings = scope.ServiceProvider.GetRequiredService<IOptions<BrevoSettings>>().Value;
        var contacts = scope.ServiceProvider.GetRequiredService<IContactService>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IMessagingDispatcher>();


        var batch = await ClaimBatchAsync(outboxDb, workerName, batchSize, ct);

        if (batch.Count == 0)
            return 0;
        foreach (var item in batch)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                using var itemScope = scopeFactory.CreateScope();
                var outboxDb2 = itemScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
                var settingsDb2 = itemScope.ServiceProvider.GetRequiredService<ApiSettingsDbContext>();
                var brevoSettings2 = itemScope.ServiceProvider.GetRequiredService<IOptions<BrevoSettings>>().Value;
                var contacts2 = itemScope.ServiceProvider.GetRequiredService<IContactService>();
                var dispatcher2 = itemScope.ServiceProvider.GetRequiredService<IMessagingDispatcher>();

                await ProcessOneAsync(logger, outboxDb2, settingsDb2, contacts2, dispatcher2, brevoSettings2, item, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing queue item {QueueId}", item.Id);

                // IMPORTANT: new scope to reset status even if previous context/connection died
                try
                {
                    using var failScope = scopeFactory.CreateScope();
                    var outboxDbFail = failScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
                    await MarkRetryOrFailAsync(outboxDbFail, item.Id, ex.Message, ct);
                }
                catch (Exception ex2)
                {
                    logger.LogError(ex2, "Failed to mark queue item retry/fail {QueueId} (will remain Sending)", item.Id);
                }
            }
        }

        return batch.Count;
    }

    /// <summary>
    /// Claims queue items (Send -> Sending) using row locks to avoid duplicates.
    /// </summary>
    private static async Task<List<WhatsAppOptinSendQueueItem>> ClaimBatchAsync(
        OutboxDbContext outboxDb,
        string workerName,
        int batchSize,
        CancellationToken ct)
    {
        await using var tx = await outboxDb.Database.BeginTransactionAsync(ct);

        var ids = await outboxDb.Database.SqlQueryRaw<Guid>($@"
SELECT TOP ({batchSize}) q.Id
FROM dbo.WhatsAppOptinSendQueue q WITH (READPAST, UPDLOCK, ROWLOCK)
WHERE q.Status = 'Send'
  AND (q.CustomerId IS NOT NULL OR NULLIF(LTRIM(RTRIM(q.Username)), '') IS NOT NULL)
ORDER BY q.Priority ASC, q.CreatedUtc ASC
").ToListAsync(ct);

        if (ids.Count == 0)
        {
            await tx.CommitAsync(ct);
            return new List<WhatsAppOptinSendQueueItem>();
        }

        // NOTE: we keep your existing approach here (string list).
        // Because Id is GUID and comes from the DB, injection risk is low.
        var idList = string.Join(",", ids.Select(x => $"'{x:D}'"));

        await outboxDb.Database.ExecuteSqlRawAsync($@"
UPDATE dbo.WhatsAppOptinSendQueue
SET Status = 'Sending',
    LockedUtc = SYSUTCDATETIME(),
    LockedBy = {{0}},
    UpdatedUtc = SYSUTCDATETIME()
WHERE Id IN ({idList});
", new object[] { workerName }, ct);

        await tx.CommitAsync(ct);

        return await outboxDb.WhatsAppOptinSendQueue
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedUtc)
            .ToListAsync(ct);
    }

    private static async Task ProcessOneAsync(
        ILogger logger,
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
            // Skip bad identifiers
            if ((!item.CustomerId.HasValue || item.CustomerId.Value <= 0) && string.IsNullOrWhiteSpace(item.Username))
            {
                await MarkFailedAsync(outboxDb, item.Id, "CustomerId and Username are both missing.", ct);
                return;
            }

            // ApiClient config
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

            // Compose contact fields: queue overrides > Nop.Customer
            var firstName = (item.FirstName ?? cust.FirstName ?? "").Trim();
            var lastName = (item.LastName ?? cust.LastName ?? "").Trim();
            var phoneRaw = (item.Phone ?? cust.Phone ?? "").Trim();
            var phoneE164 = NormalizePhoneForBrevo(phoneRaw, defaultCountryCode: "+27");


            if (string.IsNullOrWhiteSpace(firstName)) firstName = "-";
            if (string.IsNullOrWhiteSpace(lastName)) lastName = "-";


            // Create token
            var tokenId = Guid.NewGuid();
            var token = GenerateToken();
            var tokenHash = Sha256Base64(token);
            var expiresUtc = DateTime.UtcNow.AddMinutes(DefaultTtlMinutes);
            var conn = outboxDb.Database.GetDbConnection();
            logger.LogInformation("OUTBOX ConnString len={Len} DataSource='{DS}' Database='{DB}'",
                conn.ConnectionString?.Length ?? 0,
                conn.DataSource,
                conn.Database);

            // Persist token (hash) in OutboxDb
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

            await outboxDb.SaveChangesAsync(ct);

            // Build link
            var baseUrl = item.BaseUrl.Trim();
            var link = $"{baseUrl}?id={tokenId:D}&t={Uri.EscapeDataString(token)}";

            // Tracking row (WhatsAppOptinRequests)
            var requestNumber = await GetNextOptinRequestNumberAsync(outboxDb, ct);

            outboxDb.WhatsAppOptinRequests.Add(new WhatsAppOptinRequest
            {
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = null,

                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phoneE164,
                WhatsApp = null, // IMPORTANT: do NOT pre-set WhatsApp at send stage

                BrevoListId = listId,
                EmailTemplateId = item.EmailTemplateId,

                Token = token, // plaintext for tracking
                TokenExpiresOnUtc = expiresUtc,

                RequestNumber = requestNumber,
                Status = "Pending",

                ConsumedOnUtc = null,
                ConsumeIp = null,
                ConsumeUserAgent = null
            });

            await outboxDb.SaveChangesAsync(ct);

            // Upsert Brevo contact + add to list (do NOT set WHATSAPP yet)
            var brevoUpsert = await contacts.AddContactAsync(
                new ContactModel
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phoneE164,
                    WhatsApp = "" // blank until user confirms
                },
                new List<int> { listId });

            if (!brevoUpsert.Success && (brevoUpsert.Error?.Contains("Invalid phone number", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                // retry once without phone
                brevoUpsert = await contacts.AddContactAsync(
                    new ContactModel { Email = email, FirstName = firstName, LastName = lastName, Phone = "", WhatsApp = "" },
                    new List<int> { listId });
            }

            if (!brevoUpsert.Success)
            {
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

            await dispatcher.SendEmailAsync(email, item.EmailTemplateId, parameters, senderName: null, ct: ct);

            // Mark queue row Sent
            await MarkSentAsync(outboxDb, item.Id, ct);

            logger.LogInformation(
                "Opt-in email processed. QueueId={QueueId} CustomerId={CustomerId} Email={Email} RequestNumber={RequestNumber}",
                item.Id, resolvedCustomerId.Value, email, requestNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing queue item {QueueId}", item.Id);
            await MarkRetryOrFailAsync(outboxDb, item.Id, ex.Message, ct);
        }
    }

    private static async Task<int?> ResolveCustomerIdAsync(
        WhatsAppOptinSendQueueItem item,
        NopDbContext nopDb,
        CancellationToken ct)
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
        // Prefer sequence (DO NOT dispose the context-owned connection)
        try
        {
            var conn = outboxDb.Database.GetDbConnection(); // <- IMPORTANT: no using/await using

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NEXT VALUE FOR dbo.Seq_WhatsAppOptinRequestNumber;";
            cmd.CommandType = System.Data.CommandType.Text;

            var scalar = await cmd.ExecuteScalarAsync(ct);
            var next = Convert.ToInt32(scalar);

            if (next > 0)
                return next;
        }
        catch
        {
            // ignore and fallback
        }

        // Fallback: MAX+1 (serializable) - transaction disposal is fine
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

    private static async Task MarkSentAsync(OutboxDbContext outboxDb, Guid id, CancellationToken ct)
    {
        var row = await outboxDb.WhatsAppOptinSendQueue.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null) return;

        row.Status = "Sent";
        row.SentUtc = DateTime.UtcNow;
        row.UpdatedUtc = DateTime.UtcNow;
        row.LastError = null;

        row.LockedUtc = null;
        row.LockedBy = null;

        await outboxDb.SaveChangesAsync(ct);
    }

    private static async Task MarkFailedAsync(OutboxDbContext outboxDb, Guid id, string error, CancellationToken ct)
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

        await outboxDb.SaveChangesAsync(ct);
    }

    private static void EnsureOutboxConnection(OutboxDbContext outboxDb, ILogger logger)
    {
        var conn = outboxDb.Database.GetDbConnection();
        // DataSource/Database can be blank when the connection string is empty.
        if (string.IsNullOrWhiteSpace(conn.ConnectionString))
        {
            logger.LogError("OutboxDbContext connection string is empty. Check your DI registration / appsettings ConnectionStrings entry for OutboxDbContext.");
            throw new InvalidOperationException("OutboxDbContext is not configured (empty connection string). Check your appsettings ConnectionStrings and AddDbContext<OutboxDbContext> registration.");
        }
    }

    static async Task MarkRetryOrFailAsync(OutboxDbContext outboxDb, Guid id, string error, CancellationToken ct)
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

        await outboxDb.SaveChangesAsync(ct);
    }

    private static string Trunc(string s, int max)
    {
        if (string.IsNullOrWhiteSpace(s)) return s ?? "";
        return s.Length <= max ? s : s.Substring(0, max);
    }

    private static string NormalizePhoneForBrevo(string? rawPhone, string defaultCountryCode = "+27")
    {
        if (string.IsNullOrWhiteSpace(rawPhone))
            return "";

        // keep digits and leading +
        var s = rawPhone.Trim();

        // Remove common separators
        s = s.Replace(" ", "")
             .Replace("-", "")
             .Replace("(", "")
             .Replace(")", "")
             .Replace(".", "");

        // Convert leading 00 -> +
        if (s.StartsWith("00"))
            s = "+" + s.Substring(2);

        // If already E.164-ish
        if (s.StartsWith("+"))
        {
            // remove any non-digits after +
            var digits = new string(s.Skip(1).Where(char.IsDigit).ToArray());
            return digits.Length >= 7 ? "+" + digits : "";
        }

        // keep digits only
        var d = new string(s.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(d))
            return "";

        // If starts with country code without +
        if (d.StartsWith("27") && d.Length >= 9)
            return "+" + d;

        // South Africa default behavior:
        // 0832607275 -> +27832607275  (strip leading 0)
        if (defaultCountryCode == "+27")
        {
            if (d.StartsWith("0") && d.Length == 10)
                return "+27" + d.Substring(1);

            // Some people store 832607275 (missing 0)
            if (d.Length == 9)
                return "+27" + d;

            // Already has 11 digits starting with 27
            if (d.Length == 11 && d.StartsWith("27"))
                return "+27" + d.Substring(2); // (rare) normalize
        }

        // Fallback: if it looks like an international number without +
        // Only accept if at least 8 digits (Brevo will still validate)
        return d.Length >= 8 ? "+" + d : "";
    }

}
