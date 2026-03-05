using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Settings;
using AnqIntegrationApi.Services.Messaging;
using BrevoApiHelpers.Models;
using BrevoApiHelpers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Text.RegularExpressions;

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,User")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Produces("application/json")]
    public class ValidateNewRegistrationController : ControllerBase
    {
        private readonly ApiSettingsDbContext _settingsDb;
        private readonly ILogger<ValidateNewRegistrationController> _logger;
        private readonly IMessagingService _messagingService;
        private readonly IContactService _contactService;
        private readonly BrevoSettings _brevoSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessagingDispatcher _messagingDispatcher;

        public ValidateNewRegistrationController(
            ApiSettingsDbContext settingsDb,
            ILogger<ValidateNewRegistrationController> logger,
            IMessagingService messagingService,
            IContactService contactService,
            IOptions<BrevoSettings> brevoOptions,
            IHttpContextAccessor httpContextAccessor,
            IMessagingDispatcher messagingDispatcher)
        {
            _settingsDb = settingsDb;
            _logger = logger;
            _messagingService = messagingService;
            _contactService = contactService;
            _brevoSettings = brevoOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _messagingDispatcher = messagingDispatcher;
        }

        /// <summary>
        /// Validates a new registration record by ID.
        /// </summary>
        /// <remarks>
        /// <b>200 OK</b> – <c>{ status: "VALID", errors: [], sponsor?: { name, email, cell, csponsor } }</c><br/>
        /// <b>400 Bad Request</b> – <c>{ status: "INVALID", errors: [ { rule, message } ], sponsor?: { name, email, cell, csponsor } }</c><br/>
        /// <b>401 Unauthorized</b> – <c>{ status: "INVALID", errors: [ { rule: "Auth" | "Client", message } ] }</c><br/>
        /// <b>404 Not Found</b> – <c>{ status: "INVALID", errors: [ { rule: "Id", message } ] }</c><br/>
        /// <b>500 Error</b> – <c>ProblemDetails</c>
        /// </remarks>
        [HttpPost("{id}")]
        [SwaggerOperation(OperationId = "ValidateNewRegistration", Summary = "Validate a pending registration")]
        [SwaggerResponse(StatusCodes.Status200OK, "VALID result payload: { status: \"VALID\", errors: [], sponsor?: { name, email, cell } }", typeof(object))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "INVALID result payload: { status: \"INVALID\", errors: [ { rule, message } ], sponsor?: { name, email, cell } }", typeof(object))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized — missing/invalid client config.", typeof(object))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Registration ID not found.", typeof(object))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Unexpected server error.", typeof(ProblemDetails))]
        public async Task<IActionResult> ValidateNewRegistration(int id)
        {
            // Prefer ApiClient set by ApiClientContextMiddleware
            var client = _httpContextAccessor.HttpContext?.Items["ApiClient"] as ApiClient;

            if (client == null)
            {
                var claimClientKey =
    _httpContextAccessor.HttpContext?.User.FindFirst("ClientKey")?.Value ??
    _httpContextAccessor.HttpContext?.User.FindFirst("client_key")?.Value ??
    _httpContextAccessor.HttpContext?.User.FindFirst("client")?.Value ??
    _httpContextAccessor.HttpContext?.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ??
    _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


                if (string.IsNullOrWhiteSpace(claimClientKey))
                {
                    return Unauthorized(new
                    {
                        status = "INVALID",
                        errors = new[] { new { rule = "Auth", message = "Missing or invalid client key." } }
                    });
                }

                client = await _settingsDb.ApiClients.FirstOrDefaultAsync(a => a.ClientKey == claimClientKey);
                if (client == null)
                {
                    return Unauthorized(new
                    {
                        status = "INVALID",
                        errors = new[] { new { rule = "Client", message = "Client configuration not found." } }
                    });
                }
            }

            if (string.IsNullOrWhiteSpace(client.NopDbConnection) ||
                string.IsNullOrWhiteSpace(client.AccountMateDbConnection))
            {
                return Unauthorized(new
                {
                    status = "INVALID",
                    errors = new[] { new { rule = "Client", message = "Client configuration incomplete." } }
                });
            }

            var nopOptions = new DbContextOptionsBuilder<NopDbContext>()
                .UseSqlServer(client.NopDbConnection)
                .Options;
            var accountMateOptions = new DbContextOptionsBuilder<AccountMateDbContext>()
                .UseSqlServer(client.AccountMateDbConnection)
                .Options;

            using var nopDb = new NopDbContext(nopOptions);
            using var accountMateDb = new AccountMateDbContext(accountMateOptions);

            var errors = new List<object>();
            var registration = await nopDb.AnqNewRegistration.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new
                {
                    status = "INVALID",
                    errors = new[] { new { rule = "Id", message = $"Registration record with ID {id} not found." } }
                });
            }

            // --- NEW RULE: prevent duplicate new registration with same email ---
            var emailToCheck = (registration.CEmail ?? "").Trim();
            if (!string.IsNullOrEmpty(emailToCheck))
            {
                var alreadyPending = await nopDb.AnqNewRegistration
                    .Where(r =>
                        r.CEmail == registration.CEmail &&
                        r.Id != registration.Id &&
                        r.Status != "CANCELLED" &&
                        r.Status != "REJECTED")
                    .Select(r => new { r.Id, r.Status })
                    .FirstOrDefaultAsync();

                if (alreadyPending != null)
                {
                    registration.Status = "INVALID";
                    await nopDb.SaveChangesAsync();

                    return BadRequest(new
                    {
                        status = registration.Status,
                        errors = new[]
                        {
                            new {
                                rule = "Already Registered",
                                message = $"An existing registration for this email already exists (Ref #{alreadyPending.Id}, Status {alreadyPending.Status})."
                            }
                        }
                    });
                }
            }
            // --- END NEW RULE ---

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            if (string.IsNullOrWhiteSpace(registration.CEmail?.Trim()) || !emailRegex.IsMatch(registration.CEmail.Trim()))
                errors.Add(new { rule = "Email", message = "Invalid email format. " + registration.CEmail });

            if (string.IsNullOrWhiteSpace(registration.CFname))
                errors.Add(new { rule = "FirstName", message = "First name cannot be empty." });

            if (string.IsNullOrWhiteSpace(registration.CLname))
                errors.Add(new { rule = "LastName", message = "Last name cannot be empty." });

            var phoneRegex = new Regex(@"^0[6-8][0-9]{8}$");
            if (string.IsNullOrWhiteSpace(registration.CPhone1?.Trim()) || !phoneRegex.IsMatch(registration.CPhone1.Trim()))
                errors.Add(new { rule = "Cell", message = "Cell number is invalid. " + registration.CPhone1 });

            if (!string.IsNullOrWhiteSpace(registration.CPhone2?.Trim()) && !phoneRegex.IsMatch(registration.CPhone2.Trim()))
                errors.Add(new { rule = "Whatsapp", message = "WhatsApp number is invalid." });

            // Check if already registered in AccountMate
            var existingCustomer = await accountMateDb.Arcust
                .Where(c => c.Cemail == registration.CEmail)
                .Select(c => new { c.Ccustno, c.Cemail, c.Csponsor })
                .FirstOrDefaultAsync();

            if (existingCustomer != null)
            {
                var sponsor = string.IsNullOrWhiteSpace(registration.Csponsor)
                    ? await accountMateDb.Arcust
                        .Where(a => a.Ccustno == existingCustomer.Csponsor)
                        .Select(a => new
                        {
                            name = $"{(a.Cfname ?? "").Trim()} {(a.Clname ?? "").Trim()}".Trim(),
                            email = a.Cemail.Trim(),
                            cell = a.Cphone2.Trim(),
                            csponsor = a.Ccustno.Trim()
                        }).FirstOrDefaultAsync()
                    : null;

                if (sponsor != null)
                {
                    try
                    {
                        await _messagingDispatcher.SendEmailAsync(
                            sponsor.email,
                            _brevoSettings.SponsorReregistrationTemplateId,
                            new Dictionary<string, object>
                            {
                                ["ConsultantNumber"] = existingCustomer.Ccustno?.Trim(),
                                ["ConsultantFirstName"] = registration.CFname?.Trim(),
                                ["ConsultantLastName"] = registration.CLname?.Trim(),
                                ["ConsultantEmail"] = registration.CEmail?.Trim(),
                                ["ConsultantPhone"] = registration.CPhone2?.Trim()
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to notify sponsor via Brevo.");
                    }
                }

                registration.Status = "INVALID";
                await nopDb.SaveChangesAsync();

                return BadRequest(new
                {
                    status = registration.Status,
                    errors = new[]
                    {
                        new { rule = "Already Registered", message = "Email already exists in AccountMate system." }
                    },
                    ccustno = existingCustomer.Ccustno,
                    sponsor = sponsor != null ? new
                    {
                        name = sponsor.name,
                        email = sponsor.email,
                        cell = sponsor.cell
                    } : null
                });
            }

            // Sponsor validation / assignment
            if (!string.IsNullOrWhiteSpace(registration.Csponsor))
            {
                var sponsor = await accountMateDb.Arcust.FirstOrDefaultAsync(a => a.Ccustno == registration.Csponsor);
                if (sponsor == null)
                    errors.Add(new { rule = "CSponsor", message = "Sponsor does not exist in AccountMate." });
                else if (sponsor.Cstatus != "A")
                    errors.Add(new { rule = "CSponsor", message = "Sponsor exists but is not active." });
            }
            else
            {
                var postalParam = new Microsoft.Data.SqlClient.SqlParameter("@cPostCode", registration.CZip ?? (object)DBNull.Value);
                var sponsorParam = new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@CSponsor",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 20,
                    Direction = System.Data.ParameterDirection.Output
                };

                await nopDb.Database.ExecuteSqlRawAsync("EXEC ANQ_LocateRefSponsor @cPostCode, @CSponsor OUTPUT", postalParam, sponsorParam);
                registration.Csponsor = sponsorParam.Value?.ToString();

                if (string.IsNullOrWhiteSpace(registration.Csponsor))
                    errors.Add(new { rule = "CSponsor", message = "No sponsor could be assigned using postal code." });
            }

            registration.Status = errors.Any() ? "INVALID" : "VALID";
            await nopDb.SaveChangesAsync();

            object? sponsorInfo = null;

            if (registration.Status == "VALID")
            {
                sponsorInfo = await accountMateDb.Arcust
                    .Where(a => a.Ccustno == registration.Csponsor)
                    .Select(a => new
                    {
                        name = $"{(a.Cfname ?? "").Trim()} {(a.Clname ?? "").Trim()}".Trim(),
                        email = a.Cemail.Trim(),
                        cell = a.Cphone2.Trim(),
                        csponsor = a.Ccustno.Trim()
                    }).FirstOrDefaultAsync();

                // Ensure Brevo contact exists (using CleanPhoneAsync for phone/WhatsApp)
                try
                {
                    var emailNorm = (registration.CEmail ?? "").Trim().ToLowerInvariant();
                    var first = registration.CFname?.Trim();
                    var last = registration.CLname?.Trim();

                    var phoneClean = await CleanPhoneAsync(nopDb, registration.CPhone2, HttpContext.RequestAborted);
                    var waClean = await CleanPhoneAsync(nopDb, registration.CPhone3, HttpContext.RequestAborted);

                    if (!string.IsNullOrWhiteSpace(emailNorm))
                    {
                        var exists = await _contactService.ContactExistsAsync(emailNorm);
                        _logger.LogInformation("Brevo ContactExists({Email}) = {Exists}", emailNorm, exists);

                        if (!exists)
                        {
                            var contact = new ContactModel
                            {
                                Email = emailNorm,
                                FirstName = first,
                                LastName = last,
                                Phone = phoneClean,
                                WhatsApp = waClean
                            };

                            var lists = new List<int>();
                            if (_brevoSettings.NewRegistrationListId > 0)
                                lists.Add(_brevoSettings.NewRegistrationListId);

                            _logger.LogInformation("Creating Brevo contact for {Email} with lists: [{Lists}]", emailNorm, string.Join(",", lists));
                            await _contactService.AddContactAsync(contact, lists);
                        }
                        else if (_brevoSettings.NewRegistrationListId > 0)
                        {
                            try
                            {
                                await _contactService.AddContactAsync(
                                    new ContactModel { Email = emailNorm },
                                    new List<int> { _brevoSettings.NewRegistrationListId });
                            }
                            catch (Exception exList)
                            {
                                _logger.LogWarning(exList, "Failed to ensure list membership for {Email} -> {ListId}", emailNorm, _brevoSettings.NewRegistrationListId);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Brevo contact not created: registration email is empty.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Brevo contact creation failed for {Email}", registration.CEmail);
                }

                // Email to consultant with sponsor info
                await _messagingDispatcher.SendEmailAsync(
                    registration.CEmail,
                    _brevoSettings.SponsorInfoTemplateId,
                    new Dictionary<string, object>
                    {
                        { "Sponsor",      sponsorInfo?.GetType().GetProperty("csponsor")?.GetValue(sponsorInfo) },
                        { "SponsorName",  sponsorInfo?.GetType().GetProperty("name")?.GetValue(sponsorInfo) },
                        { "SponsorPhone", sponsorInfo?.GetType().GetProperty("cell")?.GetValue(sponsorInfo) },
                        { "SponsorEmail", sponsorInfo?.GetType().GetProperty("email")?.GetValue(sponsorInfo) }
                    });

                // Email the sponsor
                await _messagingDispatcher.SendEmailAsync(
                    sponsorInfo?.GetType().GetProperty("email")?.GetValue(sponsorInfo)?.ToString() ?? "",
                    _brevoSettings.SponsorNewRegistrationTemplateId,
                    new Dictionary<string, object>
                    {
                        { "ConsultantEmail",     registration.CEmail?.Trim() },
                        { "ConsultantFirstName", registration.CFname?.Trim() },
                        { "ConsultantLastName",  registration.CLname?.Trim() },
                        { "ConsultantPhone",     registration.CPhone1?.Trim() },
                        { "ConsultantWhatsapp",  registration.CPhone2?.Trim() }
                    });

                // WhatsApp the sponsor (E.164 via CleanPhoneAsync)
                try
                {
                    var sponsorCellRaw = sponsorInfo?.GetType().GetProperty("cell")?.GetValue(sponsorInfo)?.ToString();
                    var sponsorWhatsapp = await CleanPhoneAsync(nopDb, sponsorCellRaw, HttpContext.RequestAborted);
                    var sponsorName = sponsorInfo?.GetType().GetProperty("name")?.GetValue(sponsorInfo)?.ToString();
                    var sponsorEmail = sponsorInfo?.GetType().GetProperty("email")?.GetValue(sponsorInfo)?.ToString();
                    var sponsorEmailNorm = (sponsorEmail ?? "").Trim().ToLowerInvariant();
                    int? sponsorBrevoId = null;

                    if (!string.IsNullOrWhiteSpace(sponsorEmailNorm))
                    {
                        // once you've added GetContactIdByEmailAsync
                        sponsorBrevoId = await _contactService.GetContactIdByEmailAsync(sponsorEmailNorm, HttpContext.RequestAborted);
                    }


                    if (!string.IsNullOrWhiteSpace(sponsorWhatsapp))
                    {

                        await _messagingDispatcher.SendWhatsAppTemplateAsync(
 toNumber: sponsorWhatsapp,
 templateId: _brevoSettings.SponsorNewRegistrationWhatsappTemplateId,
 parameters: new Dictionary<string, object>
 {
        { "sponsorName", sponsorName?.Trim() },
        { "ConsultantEmail",     registration.CEmail?.Trim() },
        { "ConsultantFirstName", registration.CFname?.Trim() },
        { "ConsultantLastName",  registration.CLname?.Trim() },
        { "ConsultantPhone",     registration.CPhone1?.Trim() },
        { "ConsultantWhatsapp",  registration.CPhone2?.Trim() }
 },
 brevoContactId: sponsorBrevoId,
 contactEmail: sponsorEmailNorm,
 senderName: null,
 ct: HttpContext.RequestAborted
);

                        _logger.LogInformation("Sponsor WhatsApp queued to {SponsorWhatsapp} using template {TemplateName}.",
                            sponsorWhatsapp, _brevoSettings.SponsorNewRegistrationWhatsappTemplateId);
                    }
                    else
                    {
                        _logger.LogWarning("Sponsor WhatsApp not sent: no valid sponsor cell/WhatsApp found on sponsorInfo.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send sponsor WhatsApp notification.");
                }

                return Ok(new
                {
                    status = registration.Status,
                    errors,
                    sponsor = sponsorInfo
                });
            }

            // INVALID -> 400
            return BadRequest(new
            {
                status = registration.Status,
                errors,
                sponsor = (object?)null
            });
        }

        private static async Task<string?> CleanPhoneAsync(NopDbContext nopDb, string? raw, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            // 1. Run existing SQL-based cleanup first (dbo.CleanPhone)
            var conn = nopDb.Database.GetDbConnection();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT dbo.CleanPhone(@p0)";
            var p = cmd.CreateParameter();
            p.ParameterName = "@p0";
            p.Value = raw;
            p.DbType = DbType.String;
            cmd.Parameters.Add(p);

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            var result = await cmd.ExecuteScalarAsync(ct);
            var cleaned = result as string ?? result?.ToString();

            if (string.IsNullOrWhiteSpace(cleaned))
                return null;

            // 2. Strip to digits and leading '+'
            var s = new string(cleaned.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // 3. Already correct E.164?
            if (s.StartsWith("+"))
            {
                if (s.Length >= 10 && s.Length <= 16)
                    return s;
                return null;
            }

            // 4. South African local starting with 0 (0831234567)
            if (s.Length == 10 && s.StartsWith("0"))
                return "+27" + s[1..];

            // 5. Already MSISDN 27********* without '+'
            if (s.Length == 11 && s.StartsWith("27"))
                return "+" + s;

            // 6. Mobile missing its leading 0 (831234567)
            if (s.Length == 9 && "678".Contains(s[0]))
                return "+27" + s;

            // 7. Generic fallback for other intl numbers
            if (s.Length >= 9 && s.Length <= 15)
                return "+" + s;

            return null;
        }
    }
}
