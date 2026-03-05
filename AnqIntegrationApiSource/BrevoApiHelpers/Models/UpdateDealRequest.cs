using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;
public class UpdateDealRequest
{
    [Required]
    public string DealId { get; set; }

    [Required]
    public DealModel Deal { get; set; }
}