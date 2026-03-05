namespace AnqIntegrationApi.Models.Settings
{
    public class ApiClient
    {
        public int Id { get; set; }
        public string ClientKey { get; set; } = null!;
        public string JwtSigningKey { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string NopDbConnection { get; set; } = null!;
        public string AccountMateDbConnection { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? Roles { get; set; } // Comma-separated roles
    }


}