namespace BrevoApiHelpers.Models
{
    public class BrevoSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Email { get; set; } = "";
        public int NewRegistrationListId { get; set; }
        public int SponsorInfoTemplateId { get; set; }
        public int SponsorNewRegistrationTemplateId { get; set; }

        public int SponsorReregistrationTemplateId { get; set; }
        public int SponsorNewRegistrationWhatsappTemplateId { get; set; }
        public string? ForceToEmail { get; set; }
        public string? ForceToWhatsapp { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? WhatsappSenderNumber { get; set; }
        // ✅ NEW: Brevo list used for WhatsApp opt-in
        public int WhatsAppOptinListId { get; set; }
    }
}
