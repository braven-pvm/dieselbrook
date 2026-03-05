using AnqIntegrationApi.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Services
{
    public class ApiClientProvider
    {
        private readonly ApiSettingsDbContext _context;

        public ApiClientProvider(ApiSettingsDbContext context)
        {
            _context = context;
        }

        public async Task<Models.Settings.ApiClient?> GetClientByIdAsync(int id)
        {
            return await _context.ApiClients.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Models.Settings.ApiClient?> GetClientByJwtKeyAsync(string jwtKey)
        {
            return await _context.ApiClients.FirstOrDefaultAsync(c => c.JwtSigningKey == jwtKey);
        }
    }
}