/// <summary>
/// Represents a Brevo contact.
/// </summary>
using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;
public class AddContactRequest
{
    [Required]
    public ContactModel Contact { get; set; }

    [Required]
    public List<int> ListIds { get; set; }
}