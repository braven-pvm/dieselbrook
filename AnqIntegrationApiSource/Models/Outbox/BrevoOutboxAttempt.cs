namespace AnqIntegrationApi.Models.Outbox
{
    public class BrevoOutboxAttempt
    {
        public long AttemptId { get; set; }
        public long OutboxId { get; set; }
        public int AttemptNo { get; set; }
        public DateTime AttemptUtc { get; set; }

        public byte Channel { get; set; }
        public string? ToMasked { get; set; }
        public int? TemplateId { get; set; }

        public string? RequestJson { get; set; }
        public int? ResponseStatus { get; set; }
        public string? ResponseBody { get; set; }
        public string? Error { get; set; }
        public string? BrevoMessageId { get; set; }

        public int? DurationMs { get; set; }
    }
}
