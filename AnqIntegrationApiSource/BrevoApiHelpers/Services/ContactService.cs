using BrevoApiHelpers.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BrevoApiHelpers.Services
{
    public class ContactService : IContactService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContactService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public ContactService(IHttpClientFactory httpClientFactory, ILogger<ContactService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Brevo");
            _logger = logger;
        }

        public async Task<bool> ContactExistsAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"contacts/{email}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking contact existence for {email}", RedactEmail(email));
                return false;
            }
        }

        public async Task<BrevoEmailResponse> AddContactAsync(ContactModel contact, List<int>? listIds = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contact.Email))
                    throw new ArgumentException("Contact email is required.", nameof(contact));

                // Build attributes dynamically so we don't overwrite fields with blanks
                var attributes = BuildAttributes(contact);

                // payload: only include listIds when caller supplies it
                object payload = (listIds is { Count: > 0 })
                    ? new
                    {
                        email = contact.Email.Trim(),
                        attributes,
                        listIds,
                        updateEnabled = true
                    }
                    : new
                    {
                        email = contact.Email.Trim(),
                        attributes,
                        updateEnabled = true
                    };

                // 1st attempt
                var first = await PostContactsAsync(payload, contact.Email, attributes, attemptLabel: "attempt1");

                if (first.Success)
                    return first;

                // If Brevo fails due to SMS/phone issue, remove SMS and retry once
                if (IsSmsPhoneRelatedFailure(first.Error))
                {
                    _logger.LogWarning(
                        "Brevo contact upsert failed due to SMS/phone-related issue. Retrying WITHOUT SMS. Email={Email}",
                        RedactEmail(contact.Email));

                    if (attributes.Remove("SMS"))
                    {
                        object retryPayload = (listIds is { Count: > 0 })
                            ? new
                            {
                                email = contact.Email.Trim(),
                                attributes,
                                listIds,
                                updateEnabled = true
                            }
                            : new
                            {
                                email = contact.Email.Trim(),
                                attributes,
                                updateEnabled = true
                            };

                        var second = await PostContactsAsync(retryPayload, contact.Email, attributes, attemptLabel: "attempt2_no_sms");
                        return second;
                    }
                }

                return first;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding contact for {email}", RedactEmail(contact.Email));
                return new BrevoEmailResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<BrevoEmailResponse> UpdateContactAsync(string email, Dictionary<string, object> attributes)
        {
            try
            {
                var payload = new { attributes = attributes };

                var reqUri = $"contacts/{email}";
                LogRequest("PUT", reqUri, payload, email);

                var response = await _httpClient.PutAsJsonAsync(reqUri, payload);
                var content = await response.Content.ReadAsStringAsync();

                LogResponse("PUT", reqUri, (int)response.StatusCode, content, email);

                return new BrevoEmailResponse
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Error = response.IsSuccessStatusCode ? null : content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact for {email}", RedactEmail(email));
                return new BrevoEmailResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<int?> GetContactIdByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            using var resp = await _httpClient.GetAsync($"contacts/{email}", ct);

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var id))
                return id;

            return null;
        }

        // -----------------------
        // helpers
        // -----------------------

        private Dictionary<string, object> BuildAttributes(ContactModel contact)
        {
            var attributes = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(contact.FirstName))
                attributes["FIRSTNAME"] = contact.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(contact.LastName))
                attributes["LASTNAME"] = contact.LastName.Trim();

            // SMS / phone
            if (!string.IsNullOrWhiteSpace(contact.Phone))
                attributes["SMS"] = contact.Phone.Trim();

            // IMPORTANT: do NOT send WHATSAPP when empty, or you'll overwrite existing values
            if (!string.IsNullOrWhiteSpace(contact.WhatsApp))
                attributes["WHATSAPP"] = contact.WhatsApp.Trim();

            return attributes;
        }

        private async Task<BrevoEmailResponse> PostContactsAsync(object payload, string email, Dictionary<string, object> attributes, string attemptLabel)
        {
            var reqUri = "contacts";

            LogRequest("POST", reqUri, payload, email);

            var response = await _httpClient.PostAsJsonAsync(reqUri, payload);
            var content = await response.Content.ReadAsStringAsync();

            LogResponse("POST", reqUri, (int)response.StatusCode, content, email);

            return new BrevoEmailResponse
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Error = response.IsSuccessStatusCode ? null : content
            };
        }

        private void LogRequest(string method, string uri, object payload, string email)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, _jsonOpts);

                // redact common sensitive bits
                var redacted = RedactJson(json);

                _logger.LogInformation(
                    "Brevo request {Method} {Uri} Email={Email} Payload={Payload}",
                    method,
                    uri,
                    RedactEmail(email),
                    Trunc(redacted, 2000));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log Brevo request payload for Email={Email}", RedactEmail(email));
            }
        }

        private void LogResponse(string method, string uri, int statusCode, string body, string email)
        {
            try
            {
                _logger.LogInformation(
                    "Brevo response {Method} {Uri} Email={Email} Status={Status} Body={Body}",
                    method,
                    uri,
                    RedactEmail(email),
                    statusCode,
                    Trunc(body ?? "", 2000));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log Brevo response for Email={Email}", RedactEmail(email));
            }
        }

        private static bool IsSmsPhoneRelatedFailure(string? errorBody)
        {
            if (string.IsNullOrWhiteSpace(errorBody))
                return false;

            // Cover your case + broader "phone" / "sms" problems
            // Example:
            // {"code":"duplicate_parameter","message":"Unable to update contact, SMS is already associated with another Contact","metadata":{"duplicate_identifiers":["SMS"]}}
            var s = errorBody;

            return s.Contains("\"SMS\"", StringComparison.OrdinalIgnoreCase)
                || s.Contains("duplicate_identifiers", StringComparison.OrdinalIgnoreCase)
                || s.Contains("sms is already associated", StringComparison.OrdinalIgnoreCase)
                || s.Contains("sms", StringComparison.OrdinalIgnoreCase)
                || s.Contains("phone", StringComparison.OrdinalIgnoreCase);
        }

        private static string RedactEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            var at = email.IndexOf('@');
            if (at <= 1) return "***" + email[Math.Max(0, at)..];
            return email[0] + "***" + email[(at - 1)..];
        }

        private static string RedactJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;

            // quick-and-safe string-based redactions
            // email: "email":"someone@domain"
            json = System.Text.RegularExpressions.Regex.Replace(
                json,
                "(?i)(\"email\"\\s*:\\s*\")([^\"]+)(\")",
                m => m.Groups[1].Value + RedactEmail(m.Groups[2].Value) + m.Groups[3].Value);

            // SMS / phone: "SMS":"+27...." or "phone":"+27...."
            json = System.Text.RegularExpressions.Regex.Replace(
                json,
                "(?i)(\"(sms|phone|whatsapp)\"\\s*:\\s*\")([^\"]*)(\")",
                m => m.Groups[1].Value + RedactPhone(m.Groups[3].Value) + m.Groups[4].Value);

            return json;
        }

        private static string RedactPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length <= 4) return "****";
            return $"***{digits[^4..]}";
        }

        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return s ?? "";
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }
    }
}
