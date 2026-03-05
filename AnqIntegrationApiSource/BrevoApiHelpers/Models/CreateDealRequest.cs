using System.ComponentModel.DataAnnotations;


namespace BrevoApiHelpers.Models;
public class CreateDealRequest
{
    [Required]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Required]
    public DealModel? Deal { get; set; }
}