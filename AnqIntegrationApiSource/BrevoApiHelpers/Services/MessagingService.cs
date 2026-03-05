using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BrevoApiHelpers.Models;

namespace BrevoApiHelpers.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MessagingService> _logger;
        private readonly BrevoSettings _brevoSettings;

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // IMPORTANT: use IHttpClientFactory so we can select the named "Brevo" client
        public MessagingService(
            IHttpClientFactory httpClientFactory,
            IOptions<BrevoSettings> brevoOptions,
            ILogger<MessagingService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Brevo");
            _logger = logger;
            _brevoSettings = brevoOptions.Value;
        }

        public async Task<BrevoEmailResponse> SendTransactionalEmailAsync(
            string toEmail,
            int templateId,
            Dictionary<string, object> parameters)
        {
            var effectiveTo = string.IsNullOrWhiteSpace(_brevoSettings.ForceToEmail)
                ? toEmail
                : _brevoSettings.ForceToEmail;

            var payload = new
            {
                to = new[] { new { email = effectiveTo } },
                cc = new[] { new { email = "melinda@annique.com" } },
                templateId = templateId,
                @params = parameters,
                sender = new { email = _brevoSettings.SenderEmail, name = _brevoSettings.SenderName }
            };

            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOpts);

            var request = new HttpRequestMessage(HttpMethod.Post, "smtp/email")
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            var effectiveUri = _httpClient.BaseAddress is not null
                ? new Uri(_httpClient.BaseAddress, request.RequestUri!).ToString()
                : request.RequestUri?.ToString() ?? "(null)";

            _logger.LogInformation("Sending Brevo email to {EffectiveTo} (original: {Original})", effectiveTo, toEmail);
            _logger.LogInformation(
                "Brevo REQUEST:\n{Method} {Uri}\nHeaders:\n{Headers}\nContentHeaders:\n{ContentHeaders}\nBody:\n{Body}",
                request.Method,
                effectiveUri,
                RedactHeaders(JoinHeaders(request.Headers)),
                RedactHeaders(JoinHeaders(request.Content?.Headers)),
                jsonPayload);

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var (messageId, errorMsg) = ExtractMessageIdAndError(responseBody);

            _logger.Log(response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning,
                "Brevo RESPONSE: {StatusCode} ({Reason})\nHeaders:\n{Headers}\nContentHeaders:\n{ContentHeaders}\nBody:\n{Body}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                JoinHeaders(response.Headers),
                JoinHeaders(response.Content?.Headers),
                responseBody);

            return new BrevoEmailResponse
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                MessageId = messageId,
                Error = errorMsg,
                RawResponseBody = responseBody
            };
        }

        public async Task<BrevoEmailResponse> SendWhatsappTemplateAsync(
          string toNumber,
          int templateId,
          Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(_brevoSettings.WhatsappSenderNumber))
            {
                return new BrevoEmailResponse
                {
                    Success = false,
                    StatusCode = 0,
                    Error = "Brevo WhatsApp sender number not configured. Ensure Brevo:WhatsappSenderNumber is set."
                };
            }

            // Brevo requires digits only (no '+', spaces, etc.)
            static string NormalizeMsisdn(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var digits = new string(s.Where(char.IsDigit).ToArray());
                return digits;
            }

            var sender = NormalizeMsisdn(_brevoSettings.WhatsappSenderNumber);
            var recipient = NormalizeMsisdn(toNumber);

            if (string.IsNullOrWhiteSpace(recipient))
            {
                return new BrevoEmailResponse
                {
                    Success = false,
                    StatusCode = 0,
                    Error = "Brevo WhatsApp recipient number is missing/invalid."
                };
            }

            var payload = new
            {
                senderNumber = sender,
                contactNumbers = new[] { recipient },   // ✅ REQUIRED by Brevo
                templateId = templateId,
                @params = parameters
            };

            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOpts);

            var request = new HttpRequestMessage(HttpMethod.Post, "whatsapp/sendMessage")
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var (messageId, errorMsg) = ExtractMessageIdAndError(responseBody);

            return new BrevoEmailResponse
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                MessageId = messageId,
                Error = errorMsg,
                RawResponseBody = responseBody
            };
        }

        private static (string? messageId, string? error) ExtractMessageIdAndError(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                string? messageId = null;
                if (root.TryGetProperty("messageId", out var midProp) && midProp.ValueKind == JsonValueKind.String)
                    messageId = midProp.GetString();
                else if (root.TryGetProperty("messageIds", out var mids) && mids.ValueKind == JsonValueKind.Array && mids.GetArrayLength() > 0)
                    messageId = mids[0].GetString();

                string? errorMsg = null;
                if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    errorMsg = msg.GetString();
                else if (root.TryGetProperty("error", out var err))
                    errorMsg = err.ValueKind == JsonValueKind.String ? err.GetString() : err.ToString();

                return (messageId, errorMsg);
            }
            catch
            {
                return (null, null);
            }
        }

        private static string JoinHeaders(HttpHeaders? headers)
        {
            if (headers is null) return "(none)";
            var sb = new StringBuilder();
            foreach (var h in headers)
                sb.Append(h.Key).Append(": ").Append(string.Join(", ", h.Value)).Append('\n');
            return sb.ToString().TrimEnd();
        }

        private static string RedactHeaders(string headersBlock)
        {
            if (string.IsNullOrEmpty(headersBlock)) return headersBlock;
            return Redact(Redact(headersBlock, "api-key"), "authorization");
        }

        private static string Redact(string headersBlock, string headerName)
        {
            var lines = headersBlock.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var idx = lines[i].IndexOf(':');
                if (idx <= 0) continue;
                var name = lines[i].Substring(0, idx).Trim();
                if (name.Equals(headerName, StringComparison.OrdinalIgnoreCase))
                    lines[i] = $"{name}: ***";
            }
            return string.Join('\n', lines);
        }
    }
}
