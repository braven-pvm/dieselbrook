using System;
using System.ComponentModel.DataAnnotations;

namespace AnqIntegrationApi.Models.Settings
{
    public sealed class WhatsappOptInToken
    {
        [Key]
        public Guid Id { get; set; }

        public int ApiClientId { get; set; }

        public int CustomerId { get; set; }

        [MaxLength(320)]
        public string Email { get; set; } = "";

        [MaxLength(64)]
        public string Purpose { get; set; } = "WHATSAPP_OPTIN";

        [MaxLength(256)]
        public string TokenHash { get; set; } = "";

        public DateTime CreatedUtc { get; set; }

        public DateTime ExpiresUtc { get; set; }

        public DateTime? UsedUtc { get; set; }
    }
}
