
/// <summary>
/// Represents a conversation retrieved from Brevo.
/// </summary>
using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;
public class Conversation
{
    [Required]
    public string? Id { get; set; }
    [Required]
    public string? Subject { get; set; }
}