namespace AnqIntegrationApi.Models.Outbox
{
    public class BrevoWhatsappEvent
    {
        public long Id { get; set; }
        public DateTime EventUtc { get; set; }
        public string EventType { get; set; } = default!;
        public string? MessageId { get; set; }
        public string? SenderNumber { get; set; }
        public string? ContactNumber { get; set; }
        public string? Body { get; set; }
        public string? MediaUrl { get; set; }
        public string? RawJson { get; set; }
    }
}
