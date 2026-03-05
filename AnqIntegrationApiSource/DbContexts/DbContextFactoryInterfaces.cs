namespace AnqIntegrationApi.DbContexts
{
    public interface IAccountMateDbContextFactory
    {
        AccountMateDbContext CreateDbContext();
    }

    public interface INopDbContextFactory
    {
        NopDbContext CreateDbContext();
    }
}