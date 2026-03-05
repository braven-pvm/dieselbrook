using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AnqIntegrationApi.DbContexts;

namespace AnqIntegrationApi.Services
{
    public interface IClientDbContextFactory
    {
        NopDbContext CreateNopContext(string connectionString);
        AccountMateDbContext CreateAccountMateContext(string connectionString);
    }

    public class ClientDbContextFactory : IClientDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public NopDbContext CreateNopContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NopDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new NopDbContext(optionsBuilder.Options);
        }

        public AccountMateDbContext CreateAccountMateContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AccountMateDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new AccountMateDbContext(optionsBuilder.Options);
        }
    }
}