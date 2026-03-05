using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json; // ⬅️ added

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class AuthController : ControllerBase
    {
        private readonly ApiSettingsDbContext _settingsDb;
        private readonly IConfiguration _configuration;

        public AuthController(ApiSettingsDbContext settingsDb, IConfiguration configuration)
        {
            _settingsDb = settingsDb;
            _configuration = configuration;
        }

        /// <summary>
        /// Generates or updates a JWT token for the specified client key.
        /// Accepts query string OR JSON body (no DTO).
        /// </summary>
        [HttpPost("token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetToken(
            [FromQuery] string? clientKey,
            [FromQuery] string? roles = null,
            [FromQuery] bool? noExpiry = null,
            [FromBody] JsonElement? body = null) // ⬅️ optional body fallback
        {
            // Fallback to body if query values missing
            if (string.IsNullOrWhiteSpace(clientKey) && body.HasValue)
            {
                clientKey = GetString(body.Value, "clientKey") ?? GetString(body.Value, "ClientKey");
                roles = roles ?? GetString(body.Value, "roles") ?? GetString(body.Value, "Roles");
                noExpiry = noExpiry ?? GetBool(body.Value, "noExpiry") ?? GetBool(body.Value, "NoExpiry");
            }

            if (string.IsNullOrWhiteSpace(clientKey))
                return BadRequest(new { error = "clientKey is required." });

            var client = await _settingsDb.ApiClients.FirstOrDefaultAsync(c => c.ClientKey == clientKey);

            var jwtSettings = _configuration.GetSection("Jwt");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var signingKeyString = jwtSettings["SigningKey"] ?? throw new InvalidOperationException("SigningKey not configured.");
            var signingKeyBytes = Encoding.UTF8.GetBytes(signingKeyString);

            var effectiveNoExpiry = noExpiry ?? false;
            var expires = effectiveNoExpiry
                ? DateTime.UtcNow.AddYears(100)
                : DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpireMinutes"] ?? "60"));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, clientKey),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, issuer ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Aud, audience ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(roles))
            {
                foreach (var role in roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }
            else if (client != null && !string.IsNullOrWhiteSpace(client.Roles))
            {
                foreach (var role in client.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expires,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(signingKeyBytes),
                    SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            if (client == null)
            {
                client = new ApiClient
                {
                    ClientKey = clientKey,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Roles = roles ?? "User",
                    NopDbConnection = "NOP-CONNECTION-HERE",
                    AccountMateDbConnection = "AM-CONNECTION-HERE",
                    JwtSigningKey = signingKeyString,
                    Token = tokenString
                };

                _settingsDb.ApiClients.Add(client);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(roles))
                    client.Roles = roles;

                client.JwtSigningKey = signingKeyString;
                client.Token = tokenString;

                _settingsDb.ApiClients.Update(client);
            }

            await _settingsDb.SaveChangesAsync();

            return Ok(new
            {
                token = tokenString,
                clientKey = client.ClientKey,
                client.Roles,
                noExpiry = effectiveNoExpiry
            });
        }

        // ------- helpers (no DTO) -------
        private static string? GetString(JsonElement body, string name)
        {
            if (body.ValueKind != JsonValueKind.Object) return null;
            if (!body.TryGetProperty(name, out var v)) return null;
            return v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();
        }

        private static bool? GetBool(JsonElement body, string name)
        {
            if (body.ValueKind != JsonValueKind.Object) return null;
            if (!body.TryGetProperty(name, out var v)) return null;
            if (v.ValueKind == JsonValueKind.True) return true;
            if (v.ValueKind == JsonValueKind.False) return false;
            if (v.ValueKind == JsonValueKind.String && bool.TryParse(v.GetString(), out var b)) return b;
            return null;
        }
    }
}
