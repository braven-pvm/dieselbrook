using System.Security.Claims;
using AnqIntegrationApi.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Middleware
{
    public sealed class ApiClientContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiClientContextMiddleware> _logger;

        public ApiClientContextMiddleware(RequestDelegate next, ILogger<ApiClientContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApiSettingsDbContext settingsDb)
        {
            // 1) API key wins if present
            var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var client = await LoadClientByKeyAsync(settingsDb, apiKey);
                if (client != null)
                {
                    SetClientItems(context, client);
                    SetUserFromApiClient(context, client.ClientKey, client.Roles); // makes [Authorize] succeed
                }
                else
                {
                    context.Items["ApiKeyInvalid"] = true;
                    _logger.LogWarning("Invalid X-Api-Key supplied. Key='{Key}'", apiKey);
                }

                await _next(context);
                return;
            }

            // 2) JWT mode: if already authenticated, map to ApiClient and set Items["ApiClient"]
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var clientKey =
                    context.User.FindFirstValue("ClientKey")
                    ?? context.User.FindFirstValue("client_key")
                    ?? context.User.FindFirstValue("client")
                    ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirstValue("sub")
                    ?? context.User.FindFirstValue(ClaimTypes.Name);

                if (!string.IsNullOrWhiteSpace(clientKey))
                {
                    var client = await LoadClientByKeyAsync(settingsDb, clientKey);
                    if (client != null)
                    {
                        SetClientItems(context, client);
                    }
                    else
                    {
                        context.Items["JwtInvalid"] = true;
                        _logger.LogWarning("JWT authenticated but ApiClient not found for key='{Key}'", clientKey);
                    }
                }
            }

            await _next(context);
        }

        private static async Task<Models.Settings.ApiClient?> LoadClientByKeyAsync(ApiSettingsDbContext settingsDb, string key)
        {
            return await settingsDb.ApiClients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IsActive && c.ClientKey == key);
        }

        private static void SetClientItems(HttpContext context, Models.Settings.ApiClient client)
        {
            // What your controller expects
            context.Items["ApiClient"] = client;

            // Useful elsewhere
            context.Items["ApiClientKey"] = client.ClientKey;
            context.Items["NopDbConnection"] = client.NopDbConnection;
            context.Items["AccountMateDbConnection"] = client.AccountMateDbConnection;
            context.Items["JwtSigningKey"] = client.JwtSigningKey;
            context.Items["Roles"] = client.Roles;
        }

        private static void SetUserFromApiClient(HttpContext context, string clientKey, string? rolesCsv)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, clientKey),
                new Claim(ClaimTypes.Name, clientKey),

                // Matches your controller’s expected claim names
                new Claim("ClientKey", clientKey),
                new Claim("client_key", clientKey),
                new Claim("client", clientKey),
            };

            if (!string.IsNullOrWhiteSpace(rolesCsv))
            {
                foreach (var r in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    claims.Add(new Claim(ClaimTypes.Role, r));
            }

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "ApiKey"));
        }
    }

    public static class ApiClientContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiClientContext(this IApplicationBuilder app)
            => app.UseMiddleware<ApiClientContextMiddleware>();
    }
}
