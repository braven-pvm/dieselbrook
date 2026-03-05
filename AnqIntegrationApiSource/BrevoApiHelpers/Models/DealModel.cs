
/// <summary>
/// Represents a Brevo CRM deal.
/// </summary>
using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;
public class DealModel
{
    /// <summary>Name of the deal.</summary>
    [Required]
    public string Name { get; set; }

    /// <summary>Amount/value of the deal.</summary>
    public decimal? Amount { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}