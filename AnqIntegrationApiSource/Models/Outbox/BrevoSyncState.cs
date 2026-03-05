namespace AnqIntegrationApi.Models.Outbox
{
    public class BrevoSyncState
    {
        public string Name { get; set; } = default!;
        public DateTime LastEventUtc { get; set; }
        public DateTime? LastRunUtc { get; set; }
        public string? LastStatus { get; set; }
        public string? LastError { get; set; }
    }
}
