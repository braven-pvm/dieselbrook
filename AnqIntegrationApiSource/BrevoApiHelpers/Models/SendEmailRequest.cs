using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;

public class SendEmailRequest
{
    [Required]
    [EmailAddress]
    public string To { get; set; }

    [Required]
    public int TemplateId { get; set; }

    [Required]
    public Dictionary<string, object> Parameters { get; set; }
}