using System;
using System.ComponentModel.DataAnnotations;

namespace AnqIntegrationApi.Models.Outbox;

public sealed class WhatsAppOptinSendQueueItem
{
    public Guid Id { get; set; }

    public int ApiClientId { get; set; }
    public int? CustomerId { get; set; }

    public string? Username { get; set; }  // varchar(100)


    public string? Email { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }

    [Required]
    public string BaseUrl { get; set; } = "";

    public int EmailTemplateId { get; set; }
    public int? BrevoListId { get; set; }

    [Required]
    public string Status { get; set; } = "Send";

    public int Priority { get; set; } = 100;

    public int AttemptCount { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public DateTime? SentUtc { get; set; }

    public string? LastError { get; set; }

    public DateTime? LockedUtc { get; set; }
    public string? LockedBy { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}
