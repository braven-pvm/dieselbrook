using Microsoft.EntityFrameworkCore;
using AnqIntegrationApi.Models.Settings;

namespace AnqIntegrationApi.DbContexts
{
    public class ApiSettingsDbContext : DbContext
    {
        public ApiSettingsDbContext(DbContextOptions<ApiSettingsDbContext> options) : base(options) { }

        public DbSet<ApiClient> ApiClients { get; set; }

        
    }
    }