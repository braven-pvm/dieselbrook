
using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Settings;
using Microsoft.AspNetCore.Http;

namespace AnqIntegrationApi.Services
{
    public interface ISyncService
    {
        // Intentionally left empty for now; implement your sync methods as needed.
    }

    public class SyncService : ISyncService
    {
        private readonly IClientDbContextFactory _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SyncService(IClientDbContextFactory contextFactory, IHttpContextAccessor httpContextAccessor)
        {
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private ApiClient? CurrentClient =>
            _httpContextAccessor.HttpContext?.Items["ApiClient"] as ApiClient;

        /// <summary>
        /// Create an AccountMateDbContext for the current request's ApiClient.
        /// </summary>
        private AccountMateDbContext CreateAccountMateDb()
        {
            var client = CurrentClient ?? throw new InvalidOperationException("ApiClient not found on HttpContext. Ensure ApiClientContextMiddleware is registered and executing before this service.");
            if (string.IsNullOrWhiteSpace(client.AccountMateDbConnection))
                throw new InvalidOperationException("ApiClient has no AccountMateDbConnection configured.");
            return _contextFactory.CreateAccountMateContext(client.AccountMateDbConnection);
        }

        /// <summary>
        /// Create a NopDbContext for the current request's ApiClient.
        /// </summary>
        private NopDbContext CreateNopDb()
        {
            var client = CurrentClient ?? throw new InvalidOperationException("ApiClient not found on HttpContext. Ensure ApiClientContextMiddleware is registered and executing before this service.");
            if (string.IsNullOrWhiteSpace(client.NopDbConnection))
                throw new InvalidOperationException("ApiClient has no NopDbConnection configured.");
            return _contextFactory.CreateNopContext(client.NopDbConnection);
        }

        // Example usage pattern inside your future methods:
        // using var amDb = CreateAccountMateDb();
        // using var nopDb = CreateNopDb();
    }
}
