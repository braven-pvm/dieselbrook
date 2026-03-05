using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Services.Outbox;
using AnqIntegrationApi.Services.Workers;
using BrevoApiHelpers.Models;          // for BrevoSettings
using BrevoApiHelpers.Services;        // for IMessagingService, IConversationService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

#nullable enable

[ApiController]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(GroupName = "internal")]
[Route("api/[controller]")]
public class BrevoController : ControllerBase
{
    private readonly IMessagingService _messagingService;
    private readonly IConversationService _conversationService;
    private readonly IOutboxService _outbox;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BrevoSettings _brevoSettings;
    private readonly ILogger<BrevoController> _logger;

    public BrevoController(
        IMessagingService messagingService,
        IConversationService conversationService,
        IOutboxService outbox,
        IHttpClientFactory httpClientFactory,
        IOptions<BrevoSettings> brevoOptions,
        ILogger<BrevoController> logger)
    {
        _messagingService = messagingService;
        _conversationService = conversationService;
        _outbox = outbox;
        _httpClientFactory = httpClientFactory;
        _brevoSettings = brevoOptions.Value;
        _logger = logger;
    }

    /// <summary>Ping endpoint for health check.</summary>
    [HttpGet("ping")]
    [SwaggerOperation(Summary = "Health check endpoint")]
    public IActionResult Ping() => Ok("Brevo API is running");

    /// <summary>Check if a contact exists by email (direct Brevo REST).</summary>
    [HttpGet("contact/exists")]
    [SwaggerOperation(Summary = "Check if a contact exists by email")]
    public async Task<IActionResult> ContactExists([FromQuery] string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { error = "email is required." });
        var emailNorm = email.Trim().ToLowerInvariant();

        var client = GetBrevoClient();
        var resp = await client.GetAsync($"contacts/{WebUtility.UrlEncode(emailNorm)}", ct);
        return Ok(resp.IsSuccessStatusCode);
    }

    /// <summary>Update contact attributes (raw JSON, no DTOs).</summary>
    /// <remarks>
    /// {
    ///   "email": "user@example.com",
    ///   "attributes": { "FIRSTNAME":"Jane", "LASTNAME":"Doe", "SMS":"+27123456789" }
    /// }
    /// </remarks>
    [HttpPut("contact/update")]
    [SwaggerOperation(Summary = "Update attributes of an existing Brevo contact (raw JSON)")]
    public async Task<IActionResult> UpdateContact([FromBody] JsonElement body, CancellationToken ct)
    {
        var email = (GetString(body, "email") ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { error = "email is required." });

        var attributes = GetObjectDictionary(body, "attributes") ?? new();
        var client = GetBrevoClient();

        var patchBody = new { attributes };
        var resp = await client.PatchAsJsonAsync($"contacts/{WebUtility.UrlEncode(email)}", patchBody, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Brevo contact update failed ({Status}): {Body}", (int)resp.StatusCode, raw);
            return StatusCode((int)resp.StatusCode, new { error = "update_failed", details = raw });
        }

        return Ok(new { updated = true });
    }

    /// <summary>Send a transactional email (raw JSON) — sends immediately (not queued).</summary>
    /// <remarks>{ "to":"user@example.com", "templateId":12, "parameters":{ "name":"John" } }</remarks>
    [HttpPost("email/send")]
    [SwaggerOperation(Summary = "Send a transactional email (raw JSON)")]
    public async Task<IActionResult> SendEmail([FromBody] JsonElement body, CancellationToken ct)
    {
        var to = (GetString(body, "to") ?? "").Trim();
        var templateId = GetInt(body, "templateId");
        if (string.IsNullOrWhiteSpace(to) || templateId is null or <= 0)
            return BadRequest(new { error = "to and templateId are required." });

        var parameters = GetObjectDictionary(body, "parameters") ?? new();
        var resp = await _messagingService.SendTransactionalEmailAsync(to, templateId.Value, parameters);
        return Ok(resp);
    }

    /// <summary>Send a WhatsApp template (raw JSON) — sends immediately (not queued).</summary>
    /// <remarks>{ "number":"+123", "templateId":987, "parameters":{ "order":"12345" } }</remarks>
    [HttpPost("whatsapp/send")]
    [SwaggerOperation(Summary = "Send a WhatsApp template (raw JSON)")]
    public async Task<IActionResult> SendWhatsapp([FromBody] JsonElement body, CancellationToken ct)
    {
        var number = (GetString(body, "number") ?? "").Trim();
        var templateId = GetInt(body, "templateId");
        if (string.IsNullOrWhiteSpace(number) || templateId is null or <= 0)
            return BadRequest(new { error = "number and templateId are required." });

        var parameters = GetObjectDictionary(body, "parameters") ?? new();
        var resp = await _messagingService.SendWhatsappTemplateAsync(number, templateId.Value, parameters);
        return Ok(resp);
    }

    /// <summary>Get a list of conversations within a date range.</summary>
    [HttpGet("conversations")]
    [SwaggerOperation(Summary = "Get a list of conversations within a time range")]
    public async Task<IActionResult> GetConversations([FromQuery] DateTime from, [FromQuery] DateTime to) =>
        Ok(await _conversationService.GetConversationsAsync(from, to));

    /// <summary>
    /// Enqueue a Brevo message (Email or WhatsApp). Ensures the Brevo contact first (via REST).
    /// </summary>
    /// <remarks>
    /// Email:
    /// { "type":"email", "email":"user@example.com", "templateId":1234, "parameters":{...}, "firstName":"Jane", "lastName":"Doe" }
    ///
    /// WhatsApp:
    /// { "type":"whatsapp", "whatsappNo":"+27123456789", "templateId":5678, "parameters":{...}, "firstName":"Jane", "lastName":"Doe", "email":"user@example.com" }
    /// </remarks>
    [HttpPost("queue")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Queue([FromBody] JsonElement body, CancellationToken ct)
    {
        string? type = GetString(body, "type") ?? GetString(body, "Type");
        string? email = (GetString(body, "email") ?? GetString(body, "Email"))?.Trim();
        string? whatsappNo = (GetString(body, "whatsappNo") ?? GetString(body, "WhatsappNo") ?? GetString(body, "whatsapp") ?? GetString(body, "WhatsApp"))?.Trim();
        int? templateId = GetInt(body, "templateId") ?? GetInt(body, "TemplateId");
        string? firstName = (GetString(body, "firstName") ?? GetString(body, "FirstName"))?.Trim();
        string? lastName = (GetString(body, "lastName") ?? GetString(body, "LastName"))?.Trim();
        var parameters = GetObjectDictionary(body, "parameters") ?? new();

        if (string.IsNullOrWhiteSpace(type))
            return BadRequest(new { error = "type is required ('email' or 'whatsapp')." });

        var isEmail = string.Equals(type, "email", StringComparison.OrdinalIgnoreCase);
        var isWhatsapp = string.Equals(type, "whatsapp", StringComparison.OrdinalIgnoreCase);

        if (!isEmail && !isWhatsapp)
            return BadRequest(new { error = "type must be 'email' or 'whatsapp'." });

        if (templateId is null or <= 0)
            return BadRequest(new { error = "templateId is required and must be > 0." });

        if (isEmail && string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "email is required when type='email'." });

        if (isWhatsapp && string.IsNullOrWhiteSpace(whatsappNo))
            return BadRequest(new { error = "whatsappNo is required when type='whatsapp'." });

        // Ensure contact (best-effort, via Brevo REST) if an email was provided
        if (!string.IsNullOrWhiteSpace(email))
        {
            await EnsureBrevoContactAsync(
                email.Trim().ToLowerInvariant(),
                firstName,
                lastName,
                parameters.TryGetValue("SMS", out var smsObj) ? smsObj?.ToString() : null,
                parameters.TryGetValue("WHATSAPP", out var waObj) ? waObj?.ToString() : null,
                ct);
        }

        // Always enqueue
        long outboxId;
        if (isEmail)
            outboxId = await _outbox.EnqueueEmailAsync(email!, templateId.Value, parameters, senderName: null, ct);
        else
            outboxId = await _outbox.EnqueueWhatsAppTemplateAsync(whatsappNo!, templateId.Value, parameters, senderName: null, ct);

        return Accepted(new
        {
            id = outboxId,
            type = isEmail ? "email" : "whatsapp",
            email,
            whatsappNo,
            templateId,
            enqueuedUtc = DateTime.UtcNow
        });
    }


    [HttpPost("whatsapp/unsubscribe/test/{eventId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestUnsubscribe(long eventId, CancellationToken ct)
    {
        using var scope = HttpContext.RequestServices.CreateScope();

        var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        var nopDb = scope.ServiceProvider.GetRequiredService<NopDbContext>();
        var brevoHttp = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("Brevo");

        var result = await BrevoWhatsappEventsWorker.ProcessUnsubscribeByEventIdAsync(
            outboxDb, nopDb, brevoHttp, eventId, ct);

        return Ok(result);
    }

    // ===== Helpers =====

    private HttpClient GetBrevoClient()
    {
        var client = _httpClientFactory.CreateClient("brevo");
        if (client.BaseAddress == null)
        {
            client.BaseAddress = new Uri((_brevoSettings.BaseUrl ?? "https://api.brevo.com/v3/").TrimEnd('/') + "/");
            client.DefaultRequestHeaders.TryAddWithoutValidation("api-key", _brevoSettings.ApiKey);
            client.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        }
        return client;
    }

    private async Task EnsureBrevoContactAsync(string email, string? firstName, string? lastName, string? phoneSms, string? phoneWhatsapp, CancellationToken ct)
    {
        try
        {
            var client = GetBrevoClient();

            // 1) GET contact
            var getResp = await client.GetAsync($"contacts/{WebUtility.UrlEncode(email)}", ct);
            if (getResp.StatusCode == HttpStatusCode.NotFound)
            {
                // 2) CREATE
                var createBody = new
                {
                    email = email,
                    attributes = new Dictionary<string, object?>
                    {
                        ["FIRSTNAME"] = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
                        ["LASTNAME"] = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
                        ["SMS"] = string.IsNullOrWhiteSpace(phoneSms) ? null : phoneSms.Trim(),
                        ["WHATSAPP"] = string.IsNullOrWhiteSpace(phoneWhatsapp) ? null : phoneWhatsapp.Trim()
                    }
                };
                var createResp = await client.PostAsJsonAsync("contacts", createBody, ct);
                var rawCreate = await createResp.Content.ReadAsStringAsync(ct);

                if (!createResp.IsSuccessStatusCode)
                    _logger.LogWarning("Brevo contact create failed ({Status}): {Body}", (int)createResp.StatusCode, rawCreate);
                else
                    _logger.LogInformation("Brevo contact created: {Email}", email);
            }
            else if (getResp.IsSuccessStatusCode)
            {
                // 3) UPDATE attributes if provided
                var attr = new Dictionary<string, object?>();
                if (!string.IsNullOrWhiteSpace(firstName)) attr["FIRSTNAME"] = firstName.Trim();
                if (!string.IsNullOrWhiteSpace(lastName)) attr["LASTNAME"] = lastName.Trim();
                if (!string.IsNullOrWhiteSpace(phoneSms)) attr["SMS"] = phoneSms.Trim();
                if (!string.IsNullOrWhiteSpace(phoneWhatsapp)) attr["WHATSAPP"] = phoneWhatsapp.Trim();

                if (attr.Count > 0)
                {
                    var patchBody = new { attributes = attr };
                    var patchResp = await client.PatchAsJsonAsync($"contacts/{WebUtility.UrlEncode(email)}", patchBody, ct);
                    var rawPatch = await patchResp.Content.ReadAsStringAsync(ct);
                    if (!patchResp.IsSuccessStatusCode)
                        _logger.LogWarning("Brevo contact update failed ({Status}): {Body}", (int)patchResp.StatusCode, rawPatch);
                }
            }
            else
            {
                var raw = await getResp.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Brevo contact GET unexpected status {Status}: {Body}", (int)getResp.StatusCode, raw);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EnsureBrevoContact failed for {Email}", email);
        }
    }

    private static string? GetString(JsonElement obj, string name)
    {
        if (obj.ValueKind != JsonValueKind.Object) return null;
        if (!obj.TryGetProperty(name, out var v)) return null;
        return v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();
    }

    private static int? GetInt(JsonElement obj, string name)
    {
        if (obj.ValueKind != JsonValueKind.Object) return null;
        if (!obj.TryGetProperty(name, out var v)) return null;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)) return i;
        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out i)) return i;
        return null;
    }

    private static Dictionary<string, object>? GetObjectDictionary(JsonElement obj, string name)
    {
        if (obj.ValueKind != JsonValueKind.Object) return null;
        if (!obj.TryGetProperty(name, out var v)) return null;
        if (v.ValueKind != JsonValueKind.Object) return null;

        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in v.EnumerateObject())
        {
            dict[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString()!,
                JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l : (object)prop.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => prop.Value.ToString(),
                JsonValueKind.Array => prop.Value.ToString(),
                _ => prop.Value.ToString()
            };
        }
        return dict;
    }
}
