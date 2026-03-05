using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Outbox;
using AnqIntegrationApi.Models.Settings;
using AnqIntegrationApi.Services.Messaging;
using AnqIntegrationApi.Workers;
using BrevoApiHelpers.Models;
using BrevoApiHelpers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace AnqIntegrationApi.Controllers;

[ApiController]
[Route("api/whatsapp-optin")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
public sealed class WhatsappOptInController : ControllerBase
{
    private readonly ApiSettingsDbContext _settingsDb;   // ApiClients live here
    private readonly OutboxDbContext _outboxDb;          // WhatsappOptInTokens + WhatsAppOptinRequests live here
    private readonly ILogger<WhatsappOptInController> _logger;
    private readonly IHttpContextAccessor _http;

    // IMPORTANT: For dev worker launch, always use scope factory so DbContexts come from DI.
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _env;

    private readonly BrevoSettings _brevoSettings;
    private readonly IContactService _contacts;
    private readonly IMessagingDispatcher _dispatcher;

    private const int DefaultTtlMinutes = 7200;
    private const string Purpose = "WHATSAPP_OPTIN";
    private const string OptinSource = "Email Optin";

    public WhatsappOptInController(
        ApiSettingsDbContext settingsDb,
        OutboxDbContext outboxDb,
        ILogger<WhatsappOptInController> logger,
        IHttpContextAccessor http,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment env,
        IOptions<BrevoSettings> brevoOptions,
        IContactService contacts,
        IMessagingDispatcher dispatcher)
    {
        _settingsDb = settingsDb;
        _outboxDb = outboxDb;
        _logger = logger;
        _http = http;

        _scopeFactory = scopeFactory;
        _env = env;

        _brevoSettings = brevoOptions.Value;
        _contacts = contacts;
        _dispatcher = dispatcher;
    }

    // ----------------------------------------------------------------------
    // DEV: Run worker batch on-demand (Swagger visible)
    // NOTE: Uses IServiceScopeFactory so OutboxDbContext is DI-configured
    // ----------------------------------------------------------------------
    [Authorize(Roles = "Admin")]
    [HttpPost("dev/run-email-worker-once")]
    [SwaggerOperation(
        OperationId = "DevRunWhatsappOptinEmailWorkerOnce",
        Summary = "DEV ONLY: Runs one batch of the WhatsApp opt-in email worker (claims from WhatsAppOptinSendQueue).")]
    [ProducesResponseType(typeof(DevRunWorkerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DevRunEmailWorkerOnce([FromQuery] int batchSize = 25, CancellationToken ct = default)
    {
        // Hide in non-dev environments (safer)
        if (!_env.IsDevelopment())
            return NotFound();

        if (batchSize <= 0) batchSize = 25;
        if (batchSize > 200) batchSize = 200;

        var workerName = $"DEV:{Environment.MachineName}:{Guid.NewGuid():N}";
        if (workerName.Length > 64) workerName = workerName[..64];

        // CRITICAL: Do NOT pass controller-scoped DbContexts into worker.
        // Worker must resolve OutboxDbContext/ApiSettingsDbContext from DI scope.
        var claimed = await WhatsAppOptinEmailWorker.RunOnceAsync(
            scopeFactory: _scopeFactory,
            logger: _logger,
            workerName: workerName,
            batchSize: batchSize,
            ct: ct);

        return Ok(new DevRunWorkerResponse
        {
            Worker = workerName,
            BatchSize = batchSize,
            Claimed = claimed
        });
    }

    // ----------------------------------------------------------------------
    // 0) Request + send/queue opt-in email (authenticated)
    // ----------------------------------------------------------------------
    [Authorize(Roles = "Admin,User")]
    [HttpPost("request-email")]
    [SwaggerOperation(
        OperationId = "RequestWhatsappOptInEmail",
        Summary = "Create opt-in token + tracking row and send/queue the opt-in email (token included).")]
    [ProducesResponseType(typeof(RequestWhatsappOptInEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestEmail([FromBody] RequestWhatsappOptInEmailRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var client = await GetApiClientAsync(ct);
        if (client == null)
            return Unauthorized(new { message = "Client configuration not found." });

        if (string.IsNullOrWhiteSpace(req.BaseUrl))
            return BadRequest(new { message = "BaseUrl is required." });

        if (_brevoSettings.WhatsAppOptinListId <= 0)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Brevo:WhatsAppOptinListId is not configured." });

        if (string.IsNullOrWhiteSpace(client.NopDbConnection))
            return Unauthorized(new { message = "Client Nop connection not configured." });

        var nopOptions = new DbContextOptionsBuilder<NopDbContext>()
            .UseSqlServer(client.NopDbConnection)
            .Options;

        await using var nopDb = new NopDbContext(nopOptions);

        var customerId = await ResolveCustomerIdAsync(new IssueWhatsappOptInRequest
        {
            CustomerId = req.CustomerId,
            Username = req.Username
        }, nopDb, ct);

        if (customerId <= 0)
            return BadRequest(new { message = "Unable to resolve CustomerId. Provide CustomerId or a valid Username." });

        var email = await ResolveEmailAsync(new IssueWhatsappOptInRequest { Email = req.Email }, nopDb, customerId, ct);
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email could not be resolved for the customer." });

        var tokenId = Guid.NewGuid();
        var token = GenerateToken();
        var tokenHash = Sha256Base64(token);
        var expiresUtc = DateTime.UtcNow.AddMinutes(req.TtlMinutes ?? DefaultTtlMinutes);

        var tokenRow = new WhatsappOptInToken
        {
            Id = tokenId,
            ApiClientId = client.Id,
            CustomerId = customerId,
            Email = email.Trim(),
            TokenHash = tokenHash,
            Purpose = Purpose,
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = expiresUtc,
            UsedUtc = null
        };

        _outboxDb.WhatsappOptInTokens.Add(tokenRow);
        await _outboxDb.SaveChangesAsync(ct);

        var baseUrl = req.BaseUrl.Trim();
        var link = $"{baseUrl}?id={tokenId:D}&t={Uri.EscapeDataString(token)}";

        // Ensure Brevo contact exists and is in list
        var contact = new ContactModel
        {
            Email = email.Trim(),
            FirstName = req.FirstName?.Trim() ?? "",
            LastName = req.LastName?.Trim() ?? "",
            Phone = req.Phone?.Trim() ?? "",
            WhatsApp = req.WhatsApp?.Trim() ?? ""
        };

        var contactRes = await _contacts.AddContactAsync(contact, new List<int> { _brevoSettings.WhatsAppOptinListId });
        if (!contactRes.Success)
        {
            _logger.LogWarning("Brevo contact upsert/list add failed for {Email}. Status={Status} Err={Err}",
                email, contactRes.StatusCode, contactRes.Error);

            return StatusCode(StatusCodes.Status502BadGateway, new ApiErrorResponse
            {
                Code = "brevo_contact_upsert_failed",
                Message = "Brevo contact update failed.",
                Details = contactRes.Error
            });
        }

        var requestNumber = await GetNextOptinRequestNumberAsync(ct);

        var outboxRow = new WhatsAppOptinRequest
        {
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = null,

            Email = email.Trim(),
            FirstName = string.IsNullOrWhiteSpace(req.FirstName) ? null : req.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(req.LastName) ? null : req.LastName.Trim(),
            Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim(),
            WhatsApp = string.IsNullOrWhiteSpace(req.WhatsApp) ? null : req.WhatsApp.Trim(),

            BrevoListId = _brevoSettings.WhatsAppOptinListId,
            EmailTemplateId = req.EmailTemplateId,

            Token = token,
            TokenExpiresOnUtc = expiresUtc,

            RequestNumber = requestNumber,
            Status = "Pending"
        };

        _outboxDb.WhatsAppOptinRequests.Add(outboxRow);
        await _outboxDb.SaveChangesAsync(ct);

        var parameters = req.Parameters is not null
            ? new Dictionary<string, object>(req.Parameters)
            : new Dictionary<string, object>();

        parameters["id"] = tokenId.ToString("D");
        parameters["token"] = token;
        parameters["link"] = link;
        parameters["email"] = email.Trim();
        parameters["firstName"] = contact.FirstName ?? "";
        parameters["lastName"] = contact.LastName ?? "";
        parameters["whatsApp"] = contact.WhatsApp ?? "";

        await _dispatcher.SendEmailAsync(email.Trim(), req.EmailTemplateId, parameters, senderName: null, ct: ct);

        return Ok(new RequestWhatsappOptInEmailResponse
        {
            CustomerId = customerId,
            EmailMasked = MaskEmail(email),
            ExpiresUtc = expiresUtc,
            Link = link,
            RequestNumber = requestNumber
        });
    }

    // ----------------------------------------------------------------------
    // 1) Issue link (authenticated) - generate link only (no email send)
    // ----------------------------------------------------------------------
    [Authorize(Roles = "Admin,User")]
    [HttpPost("issue")]
    [SwaggerOperation(
        OperationId = "IssueWhatsappOptInLink",
        Summary = "Create a single-use WhatsApp opt-in token link (no email send).")]
    [ProducesResponseType(typeof(IssueWhatsappOptInResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Issue([FromBody] IssueWhatsappOptInRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var client = await GetApiClientAsync(ct);
        if (client == null)
            return Unauthorized(new { message = "Client configuration not found." });

        if (string.IsNullOrWhiteSpace(client.NopDbConnection))
            return Unauthorized(new { message = "Client Nop connection not configured." });

        var nopOptions = new DbContextOptionsBuilder<NopDbContext>()
            .UseSqlServer(client.NopDbConnection)
            .Options;

        await using var nopDb = new NopDbContext(nopOptions);

        var customerId = await ResolveCustomerIdAsync(req, nopDb, ct);
        if (customerId <= 0)
            return BadRequest(new { message = "Unable to resolve CustomerId. Provide CustomerId or a valid Username." });

        var email = await ResolveEmailAsync(req, nopDb, customerId, ct);
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email could not be resolved for the customer." });

        var tokenId = Guid.NewGuid();
        var token = GenerateToken();
        var tokenHash = Sha256Base64(token);
        var expiresUtc = DateTime.UtcNow.AddMinutes(req.TtlMinutes ?? DefaultTtlMinutes);

        var row = new WhatsappOptInToken
        {
            Id = tokenId,
            ApiClientId = client.Id,
            CustomerId = customerId,
            Email = email.Trim(),
            TokenHash = tokenHash,
            Purpose = Purpose,
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = expiresUtc,
            UsedUtc = null
        };

        _outboxDb.WhatsappOptInTokens.Add(row);
        await _outboxDb.SaveChangesAsync(ct);

        var baseUrl = req.BaseUrl.Trim();
        var link = $"{baseUrl}?id={tokenId:D}&t={Uri.EscapeDataString(token)}";

        return Ok(new IssueWhatsappOptInResponse
        {
            CustomerId = customerId,
            EmailMasked = MaskEmail(email),
            ExpiresUtc = expiresUtc,
            Link = link
        });
    }

    // ----------------------------------------------------------------------
    // 2) Context (anonymous)
    // ----------------------------------------------------------------------
    [AllowAnonymous]
    [HttpGet("context")]
    [SwaggerOperation(
     OperationId = "GetWhatsappOptInContext",
     Summary = "Validate token and return masked context for the opt-in page.")]
    [ProducesResponseType(typeof(WhatsappOptInContextResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContext([FromQuery] Guid id, [FromQuery] string t, CancellationToken ct)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(t))
            return BadRequest(new { message = "Missing id or token." });

        var row = await _outboxDb.WhatsappOptInTokens.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null)
            return NotFound(new { message = "Token not found." });

        if (row.UsedUtc != null)
            return BadRequest(new { message = "This link has already been used." });

        if (row.ExpiresUtc < DateTime.UtcNow)
            return BadRequest(new { message = "This link has expired." });

        if (!SlowEquals(Sha256Base64(t), row.TokenHash))
            return Unauthorized(new { message = "Invalid token." });

        // NEW: fetch existing WhatsApp from Nop DB (best effort)
        string? existingWhatsapp = null;
        try
        {
            var client = await _settingsDb.ApiClients
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == row.ApiClientId, ct);

            if (client != null && !string.IsNullOrWhiteSpace(client.NopDbConnection))
            {
                var nopOptions = new DbContextOptionsBuilder<NopDbContext>()
                    .UseSqlServer(client.NopDbConnection)
                    .Options;

                await using var nopDb = new NopDbContext(nopOptions);

                existingWhatsapp = await nopDb.AnqUserProfileAdditionalInfos
                    .AsNoTracking()
                    .Where(x => x.CustomerId == row.CustomerId)
                    .Select(x => x.WhatsappNumber)
                    .FirstOrDefaultAsync(ct);

                existingWhatsapp = string.IsNullOrWhiteSpace(existingWhatsapp) ? null : existingWhatsapp.Trim();
            }
        }
        catch (Exception ex)
        {
            // best effort - do not fail context page
            _logger.LogWarning(ex, "Failed to load existing WhatsApp number for CustomerId {CustomerId}", row.CustomerId);
        }

        return Ok(new WhatsappOptInContextResponse
        {
            CustomerId = row.CustomerId,
            EmailMasked = MaskEmail(row.Email ?? ""),
            ExpiresUtc = row.ExpiresUtc,
            ExistingWhatsappNumber = existingWhatsapp
        });
    }

    // ----------------------------------------------------------------------
    // 3) Confirm (anonymous)
    // ----------------------------------------------------------------------
    [AllowAnonymous]
    [HttpPost("confirm")]
    [SwaggerOperation(
        OperationId = "ConfirmWhatsappOptIn",
        Summary = "Confirm WhatsApp opt-in/out and update Brevo + Nop (single-use token).")]
    [ProducesResponseType(typeof(ConfirmWhatsappOptInResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Confirm([FromBody] ConfirmWhatsappOptInRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Token lookup + validation (same for both flows)
        var row = await _outboxDb.WhatsappOptInTokens.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (row == null)
            return NotFound(new { message = "Token not found." });

        if (row.UsedUtc != null)
            return BadRequest(new { message = "This link has already been used." });

        if (row.ExpiresUtc < DateTime.UtcNow)
            return BadRequest(new { message = "This link has expired." });

        if (!SlowEquals(Sha256Base64(req.Token), row.TokenHash))
            return Unauthorized(new { message = "Invalid token." });

        var nowUtc = DateTime.UtcNow;

        // ApiClient (settings DB)
        var client = await _settingsDb.ApiClients.FirstOrDefaultAsync(a => a.Id == row.ApiClientId, ct);
        if (client == null)
            return Unauthorized(new { message = "Client configuration not found." });

        if (string.IsNullOrWhiteSpace(client.NopDbConnection))
            return Unauthorized(new { message = "Client Nop connection not configured." });

        // Build Nop context ONCE
        var nopOptions = new DbContextOptionsBuilder<NopDbContext>()
            .UseSqlServer(client.NopDbConnection)
            .Options;

        await using var nopDb = new NopDbContext(nopOptions);

        // Load FirstName/LastName/Phone from Customer table (for Brevo)
        var cust = await nopDb.Customers
            .AsNoTracking()
            .Where(c => c.Id == row.CustomerId)
            .Select(c => new { c.FirstName, c.LastName, c.Phone })
            .FirstOrDefaultAsync(ct);

        var firstName = cust?.FirstName?.Trim() ?? "";
        var lastName = cust?.LastName?.Trim() ?? "";
        var phone = cust?.Phone?.Trim() ?? "";

        // Optional fallback: tracking row
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(phone))
        {
            var reqRow = await _outboxDb.WhatsAppOptinRequests
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOnUtc)
                .FirstOrDefaultAsync(x => x.Token == req.Token && x.Email == row.Email, ct);

            if (string.IsNullOrWhiteSpace(firstName)) firstName = reqRow?.FirstName?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(lastName)) lastName = reqRow?.LastName?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(phone)) phone = reqRow?.Phone?.Trim() ?? "";
        }

        if (string.IsNullOrWhiteSpace(firstName)) firstName = "-";
        if (string.IsNullOrWhiteSpace(lastName)) lastName = "-";
        phone ??= "";

        // ------------------------------------------------------------------
        // BRANCH A: OPT-IN (keep existing flow + validate WhatsApp number)
        // ------------------------------------------------------------------
        if (req.WhatsappOptin)
        {
            var wa = (req.WhatsappE164 ?? "").Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(wa, @"^\+[1-9]\d{7,15}$"))
                return BadRequest(new { message = "WhatsApp number must be in E.164 format (e.g. +4479...)." });

            // STEP 1: Brevo FIRST - ensure contact with WhatsApp number
            var ensureRes = await _contacts.AddContactAsync(
                new ContactModel
                {
                    Email = row.Email?.Trim(),
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    WhatsApp = wa
                },
                new List<int>() // empty list => do not modify lists
            );

            if (!ensureRes.Success)
            {
                if (IsBrevoDuplicateWhatsapp(ensureRes.Error))
                {
                    return Conflict(new ApiErrorResponse
                    {
                        Code = "brevo_whatsapp_in_use",
                        Message = "That WhatsApp number is already linked to another profile. Please enter a different number.",
                        Details = new { field = "WhatsappNumber" }
                    });
                }

                _logger.LogError("Brevo upsert failed for {Email}. Status={Status}. Err={Err}",
                    row.Email, ensureRes.StatusCode, ensureRes.Error);

                return StatusCode(StatusCodes.Status502BadGateway, new ApiErrorResponse
                {
                    Code = "brevo_upsert_failed",
                    Message = "We couldn’t update your WhatsApp preferences right now. Please try again later.",
                    Details = ensureRes.Error
                });
            }

            // Consent attributes
            var brevoRes = await _contacts.UpdateContactAsync(
                row.Email!.Trim(),
                new Dictionary<string, object>
                {
                    ["WHATSAPP_CONSENT"] = true,
                    ["WHATSAPP_OPTINDATE"] = nowUtc.ToString("O"),
                    ["WHATSAPP_OPTINSOURCE"] = OptinSource
                });

            if (!brevoRes.Success)
            {
                _logger.LogError("Brevo consent update failed for {Email}. Status={Status}. Err={Err}",
                    row.Email, brevoRes.StatusCode, brevoRes.Error);

                return StatusCode(StatusCodes.Status502BadGateway, new ApiErrorResponse
                {
                    Code = "brevo_update_failed",
                    Message = "We couldn’t update your WhatsApp preferences right now. Please try again later.",
                    Details = brevoRes.Error
                });
            }

            // STEP 2: Update Nop opt-in fields + WhatsApp number
            var nopSql = @"
IF EXISTS (SELECT 1 FROM dbo.ANQ_UserProfileAdditionalInfo WHERE CustomerId = {0})
BEGIN
    UPDATE dbo.ANQ_UserProfileAdditionalInfo
       SET WhatsappNumber = {1},
           whatsappoptin = 1,
           whatsappoptindate = SYSUTCDATETIME(),
           whatsappoptinsource = 'Email Optin'
     WHERE CustomerId = {0};
END
ELSE
BEGIN
    INSERT INTO dbo.ANQ_UserProfileAdditionalInfo
        (CustomerId, WhatsappNumber, whatsappoptin, whatsappoptindate, whatsappoptinsource)
    VALUES
        ({0}, {1}, 1, SYSUTCDATETIME(), 'Email Optin');
END
";
            await nopDb.Database.ExecuteSqlRawAsync(nopSql, new object[] { row.CustomerId, wa }, ct);

            // STEP 3: Consume token after both succeeded
            row.UsedUtc = nowUtc;
            await _outboxDb.SaveChangesAsync(ct);

            // STEP 4: Update tracking row (best effort)
            await TryConsumeTrackingRowAsync(req.Token, row.Email, nowUtc, ct);

            return Ok(new ConfirmWhatsappOptInResponse
            {
                Status = "OK",
                CustomerId = row.CustomerId,
                EmailMasked = MaskEmail(row.Email ?? ""),
                WhatsappNumber = wa,
                ConsentUtc = nowUtc
            });
        }

        // ------------------------------------------------------------------
        // BRANCH B: OPT-OUT (no WhatsApp number validations)
        // ------------------------------------------------------------------

        // Brevo: best effort (but you probably still want to fail if Brevo is down)
        // We do NOT set WhatsApp number here.
        var ensureContactRes = await _contacts.AddContactAsync(
            new ContactModel
            {
                Email = row.Email?.Trim(),
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                WhatsApp = "" // do not change WA number
            },
            new List<int>() // empty list => do not modify lists
        );

        if (!ensureContactRes.Success)
        {
            _logger.LogError("Brevo ensure contact failed for {Email}. Status={Status}. Err={Err}",
                row.Email, ensureContactRes.StatusCode, ensureContactRes.Error);

            return StatusCode(StatusCodes.Status502BadGateway, new ApiErrorResponse
            {
                Code = "brevo_upsert_failed",
                Message = "We couldn’t update your WhatsApp preferences right now. Please try again later.",
                Details = ensureContactRes.Error
            });
        }

        var brevoOptOutRes = await _contacts.UpdateContactAsync(
            row.Email!.Trim(),
            new Dictionary<string, object>
            {
                ["WHATSAPP_CONSENT"] = false,
                // optional attributes if you use them:
                ["WHATSAPP_OPTOUTDATE"] = nowUtc.ToString("O"),
                ["WHATSAPP_OPTOUTSOURCE"] = OptinSource
            });

        if (!brevoOptOutRes.Success)
        {
            _logger.LogError("Brevo opt-out update failed for {Email}. Status={Status}. Err={Err}",
                row.Email, brevoOptOutRes.StatusCode, brevoOptOutRes.Error);

            return StatusCode(StatusCodes.Status502BadGateway, new ApiErrorResponse
            {
                Code = "brevo_update_failed",
                Message = "We couldn’t update your WhatsApp preferences right now. Please try again later.",
                Details = brevoOptOutRes.Error
            });
        }

        // Nop: set whatsappoptin = 0, do NOT require WhatsappNumber, and do NOT overwrite it
        var nopSqlOptOut = @"
IF EXISTS (SELECT 1 FROM dbo.ANQ_UserProfileAdditionalInfo WHERE CustomerId = {0})
BEGIN
    UPDATE dbo.ANQ_UserProfileAdditionalInfo
       SET whatsappoptin = 0,
           whatsappoptindate = SYSUTCDATETIME(),
           whatsappoptinsource = 'Opted Out'
     WHERE CustomerId = {0};
END
ELSE
BEGIN
    INSERT INTO dbo.ANQ_UserProfileAdditionalInfo
        (CustomerId, WhatsappNumber, whatsappoptin, whatsappoptindate, whatsappoptinsource)
    VALUES
        ({0}, NULL, 0, SYSUTCDATETIME(), 'Opted Out');
END
";
        await nopDb.Database.ExecuteSqlRawAsync(nopSqlOptOut, new object[] { row.CustomerId }, ct);

        // Consume token after both succeeded
        row.UsedUtc = nowUtc;
        await _outboxDb.SaveChangesAsync(ct);

        await TryConsumeTrackingRowAsync(req.Token, row.Email, nowUtc, ct);

        return Ok(new ConfirmWhatsappOptInResponse
        {
            Status = "OK",
            CustomerId = row.CustomerId,
            EmailMasked = MaskEmail(row.Email ?? ""),
            WhatsappNumber = "",          // no number provided/validated
            ConsentUtc = nowUtc
        });
    }

    // Helper: keep your existing tracking update logic but extracted
    private async Task TryConsumeTrackingRowAsync(string token, string? email, DateTime nowUtc, CancellationToken ct)
    {
        try
        {
            var outboxRow = await _outboxDb.WhatsAppOptinRequests
                .OrderByDescending(x => x.CreatedOnUtc)
                .FirstOrDefaultAsync(x => x.Token == token && x.Email == email, ct);

            if (outboxRow != null && outboxRow.Status == "Pending")
            {
                outboxRow.Status = "Consumed";
                outboxRow.ConsumedOnUtc = nowUtc;
                outboxRow.ConsumeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                outboxRow.ConsumeUserAgent = Request.Headers.UserAgent.ToString();
                outboxRow.UpdatedOnUtc = nowUtc;

                await _outboxDb.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update WhatsAppOptinRequests status for token/email.");
        }
    }


    // ----------------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------------
    private static async Task<int> ResolveCustomerIdAsync(IssueWhatsappOptInRequest req, NopDbContext nopDb, CancellationToken ct)
    {
        if (req.CustomerId.HasValue && req.CustomerId.Value > 0)
            return req.CustomerId.Value;

        if (!string.IsNullOrWhiteSpace(req.Username))
        {
            var uname = req.Username.Trim();
            var id = await nopDb.Customers
                .Where(c => c.Username == uname)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(ct);

            return id;
        }

        return 0;
    }

    private static async Task<string?> ResolveEmailAsync(IssueWhatsappOptInRequest req, NopDbContext nopDb, int customerId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(req.Email))
            return req.Email.Trim();

        var email = await nopDb.Customers
            .Where(c => c.Id == customerId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync(ct);

        return email?.Trim();
    }

    private async Task<ApiClient?> GetApiClientAsync(CancellationToken ct)
    {
        if (_http.HttpContext?.Items.TryGetValue("ApiClient", out var obj) == true && obj is ApiClient ac)
            return ac;

        var claimClientKey =
            _http.HttpContext?.User.FindFirst("ClientKey")?.Value ??
            _http.HttpContext?.User.FindFirst("client_key")?.Value ??
            _http.HttpContext?.User.FindFirst("client")?.Value;

        if (string.IsNullOrWhiteSpace(claimClientKey))
            return null;

        return await _settingsDb.ApiClients.FirstOrDefaultAsync(a => a.ClientKey == claimClientKey, ct);
    }

    private async Task<int> GetNextOptinRequestNumberAsync(CancellationToken ct)
    {
        try
        {
            var next = await _outboxDb.Database
                .SqlQueryRaw<int>("SELECT NEXT VALUE FOR dbo.Seq_WhatsAppOptinRequestNumber AS Value")
                .SingleAsync(ct);

            if (next > 0)
                return next;
        }
        catch
        {
            // ignore and fallback
        }

        await using var tx = await _outboxDb.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

        var max = await _outboxDb.WhatsAppOptinRequests.MaxAsync(x => (int?)x.RequestNumber, ct) ?? 0;
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

    private static bool SlowEquals(string a, string b)
    {
        if (a == null || b == null) return false;

        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);

        uint diff = (uint)ba.Length ^ (uint)bb.Length;
        var len = Math.Min(ba.Length, bb.Length);
        for (int i = 0; i < len; i++)
            diff |= (uint)(ba[i] ^ bb[i]);

        return diff == 0;
    }

    private static string MaskEmail(string email)
    {
        try
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return "***";

            var user = parts[0];
            var domain = parts[1];

            var maskedUser =
                user.Length <= 1 ? "*" :
                user.Length == 2 ? $"{user[0]}*" :
                $"{user[0]}***{user[^1]}";

            return $"{maskedUser}@{domain}";
        }
        catch
        {
            return "***";
        }
    }

    private static bool IsBrevoDuplicateWhatsapp(string? brevoError)
    {
        if (string.IsNullOrWhiteSpace(brevoError))
            return false;

        return brevoError.Contains("\"code\":\"duplicate_parameter\"", StringComparison.OrdinalIgnoreCase)
            && brevoError.Contains("WHATSAPP", StringComparison.OrdinalIgnoreCase);
    }
}

// ======================================================================
// DTOs
// ======================================================================

public sealed class RequestWhatsappOptInEmailRequest
{
    [Required]
    public string BaseUrl { get; set; } = "";

    public int? CustomerId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }

    [Range(5, 7200)]
    public int? TtlMinutes { get; set; }

    [Required]
    public int EmailTemplateId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }

    public Dictionary<string, object>? Parameters { get; set; }
}

public sealed class RequestWhatsappOptInEmailResponse
{
    public int CustomerId { get; set; }
    public string EmailMasked { get; set; } = "";
    public DateTime ExpiresUtc { get; set; }
    public string Link { get; set; } = "";
    public int RequestNumber { get; set; }
}

public sealed class IssueWhatsappOptInRequest
{
    [Required]
    public string BaseUrl { get; set; } = "";

    public int? CustomerId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }

    [Range(5, 7200)]
    public int? TtlMinutes { get; set; }
}

public sealed class IssueWhatsappOptInResponse
{
    public int CustomerId { get; set; }
    public string EmailMasked { get; set; } = "";
    public DateTime ExpiresUtc { get; set; }
    public string Link { get; set; } = "";
}

public sealed class WhatsappOptInContextResponse
{
    public int CustomerId { get; set; }
    public string EmailMasked { get; set; } = "";
    public DateTime ExpiresUtc { get; set; }
    // NEW: existing WhatsApp (from Nop ANQ_UserProfileAdditionalInfo.WhatsappNumber)
    public string? ExistingWhatsappNumber { get; set; }
}

public sealed class ConfirmWhatsappOptInRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = "";

    // Was [Required] — must be optional for opt-out
    public string? WhatsappE164 { get; set; }

    public bool WhatsappOptin { get; set; }
}


public sealed class ConfirmWhatsappOptInResponse
{
    public string Status { get; set; } = "OK";
    public int CustomerId { get; set; }
    public string EmailMasked { get; set; } = "";
    public string WhatsappNumber { get; set; } = "";
    public DateTime ConsentUtc { get; set; }
}

public sealed class ApiErrorResponse
{
    public string Code { get; set; } = "error";
    public string Message { get; set; } = "";
    public object? Details { get; set; }
}

public sealed class DevRunWorkerResponse
{
    public string Worker { get; set; } = "";
    public int BatchSize { get; set; }
    public int Claimed { get; set; }
}
