
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnqIntegrationApi.Models.Outbox
{
    public enum BrevoOutboxType { Email = 1, WhatsApp = 2 }
    public enum BrevoOutboxStatus { Pending = 0, InProgress = 1, Sent = 2, Failed = 3 }

    [Table("BrevoOutbox")]
    public class BrevoOutboxMessage
    {
        [Key] public long Id { get; set; }
        [Required] public BrevoOutboxType Type { get; set; }
        [Required, MaxLength(256)] public string To { get; set; } = default!;
        public int? TemplateId { get; set; }
        [Required] public string ParamsJson { get; set; } = "{}";
        [MaxLength(200)] public string? SenderName { get; set; }
        public BrevoOutboxStatus Status { get; set; } = BrevoOutboxStatus.Pending;
        public int Attempts { get; set; } = 0;
        [MaxLength(4000)] public string? LastError { get; set; }
        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? SentUtc { get; set; }
        public DateTimeOffset? NextAttemptUtc { get; set; }

        public int? BrevoContactID { get; set; }
    }
}
