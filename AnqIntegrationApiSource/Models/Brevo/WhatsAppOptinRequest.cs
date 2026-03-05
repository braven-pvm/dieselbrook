namespace AnqIntegrationApi.Models.Outbox
{
    public class WhatsAppOptinRequest
    {
        public int Id { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public string Email { get; set; } = default!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? WhatsApp { get; set; }

        public int BrevoListId { get; set; }
        public int EmailTemplateId { get; set; }

        public string Token { get; set; } = default!;
        public DateTime? TokenExpiresOnUtc { get; set; }

        public int RequestNumber { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Consumed / Expired / Cancelled
        public DateTime? ConsumedOnUtc { get; set; }
        public string? ConsumeIp { get; set; }
        public string? ConsumeUserAgent { get; set; }
    }
}
