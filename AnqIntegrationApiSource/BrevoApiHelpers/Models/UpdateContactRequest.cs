using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;
public class UpdateContactRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public Dictionary<string, object> Attributes { get; set; }
}