using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;
public class SendWhatsappRequest
{
    [Required]
    public string Number { get; set; }

    [Required]
    public int TemplateId { get; set; }

    [Required]
    public Dictionary<string, object> Parameters { get; set; }
}